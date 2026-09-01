using AutoMapper;
using PosApi.DTOs.Cash;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

/// <summary>
/// Cashier Day/Shift cash-drawer closing workflow.
///
/// Expected Cash = OpeningCash + (this cashier's completed Cash payments at this branch since
/// OpenedAt) - (this cashier's expenses at this branch since OpeningAt's date). It is computed
/// fresh every time a Recalculate or Close is attempted, so any invoice/expenditure the user adds
/// after opening the shift (to fix a shortage/excess) is automatically picked up on the next
/// attempt - there is nothing else to "apply" for a fix, the caller simply calls Recalculate again.
/// </summary>
public class CashierShiftService : ICashierShiftService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CashierShiftService> _logger;

    public CashierShiftService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CashierShiftService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CashierShiftDto>> SearchAsync(
        string? branchCode,
        string? cashierCode,
        CashierShiftStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var shifts = await _unitOfWork.CashierShifts.SearchAsync(branchCode, cashierCode, status, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<CashierShiftDto>>(shifts);
    }

    public async Task<CashierShiftDto> GetByIdAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        var shift = await _unitOfWork.CashierShifts.GetByIdWithDetailsAsync(shiftId, cancellationToken)
            ?? throw new NotFoundException("CashierShift", shiftId);

        return _mapper.Map<CashierShiftDto>(shift);
    }

    public async Task<IReadOnlyList<CashierShiftHistoryDto>> GetHistoryAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.CashierShifts.GetByIdAsync(shiftId, cancellationToken) is null)
        {
            throw new NotFoundException("CashierShift", shiftId);
        }

        var history = await _unitOfWork.CashierShiftHistories.GetByShiftIdAsync(shiftId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CashierShiftHistoryDto>>(history);
    }

    public async Task<CashierShiftDto> OpenShiftAsync(OpenCashierShiftDto request, string cashierCode, CancellationToken cancellationToken = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(request.BranchCode, cancellationToken)
            ?? throw new NotFoundException("Branch", request.BranchCode);

        if (await _unitOfWork.CashierShifts.GetOpenShiftAsync(cashierCode, branch.BranchCode, cancellationToken) is not null)
        {
            throw new ConflictException(
                $"Cashier '{cashierCode}' already has an open shift at branch '{branch.BranchCode}'. Close it before starting a new one.");
        }

        var shift = new CashierShift
        {
            BranchCode = branch.BranchCode,
            CashierCode = cashierCode,
            OpeningCash = request.OpeningCash,
            OpenedAt = DateTime.UtcNow,
            Status = CashierShiftStatus.Open
        };

        await _unitOfWork.CashierShifts.AddAsync(shift, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // assigns ShiftId, needed by the history row below

        await _unitOfWork.CashierShiftHistories.AddAsync(new CashierShiftHistory
        {
            ShiftId = shift.ShiftId,
            Action = CashierShiftHistoryAction.Opened,
            ExpectedCash = shift.OpeningCash,
            ChangedBy = cashierCode,
            ChangedAt = DateTime.UtcNow,
            Remarks = $"Shift opened with opening cash {request.OpeningCash:N2}."
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cashier shift {ShiftId} opened for cashier {CashierCode} at branch {BranchCode} with opening cash {OpeningCash:N2}",
            shift.ShiftId, cashierCode, branch.BranchCode, request.OpeningCash);

        return await GetByIdAsync(shift.ShiftId, cancellationToken);
    }

    public async Task<CashierShiftDto> RecalculateAsync(int shiftId, RecalculateCashierShiftDto request, string performedBy, CancellationToken cancellationToken = default)
    {
        var shift = await _unitOfWork.CashierShifts.GetByIdAsync(shiftId, cancellationToken)
            ?? throw new NotFoundException("CashierShift", shiftId);

        EnsureOpen(shift);

        var expectedCash = await ComputeExpectedCashAsync(shift, cancellationToken);
        var differenceAmount = Math.Round(request.ActualCash - expectedCash, 2);

        if (differenceAmount == 0)
        {
            return await CloseInternalAsync(
                shift, request.ActualCash, expectedCash, 0, null, null,
                CashierShiftHistoryAction.ClosedBalanced, performedBy, cancellationToken);
        }

        shift.ExpectedCash = expectedCash;
        shift.ActualCash = request.ActualCash;
        shift.DifferenceAmount = differenceAmount;
        // Any previously recorded reason no longer applies to this fresh recalculation - the
        // caller must choose again (fix further, or close with a reason) based on this new snapshot.
        shift.ReasonType = null;
        shift.ReasonDescription = null;
        _unitOfWork.CashierShifts.Update(shift);

        await _unitOfWork.CashierShiftHistories.AddAsync(new CashierShiftHistory
        {
            ShiftId = shift.ShiftId,
            Action = CashierShiftHistoryAction.Recalculated,
            ExpectedCash = expectedCash,
            ActualCash = request.ActualCash,
            DifferenceAmount = differenceAmount,
            ChangedBy = performedBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = $"Recalculated - {(differenceAmount > 0 ? "an excess" : "a shortage")} of {Math.Abs(differenceAmount):N2} remains."
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cashier shift {ShiftId} recalculated by {PerformedBy}: expected {ExpectedCash:N2}, actual {ActualCash:N2}, difference {DifferenceAmount:N2}",
            shift.ShiftId, performedBy, expectedCash, request.ActualCash, differenceAmount);

        return await GetByIdAsync(shift.ShiftId, cancellationToken);
    }

    public async Task<CashierShiftDto> CloseAsync(int shiftId, CloseCashierShiftDto request, string closedBy, CancellationToken cancellationToken = default)
    {
        var shift = await _unitOfWork.CashierShifts.GetByIdAsync(shiftId, cancellationToken)
            ?? throw new NotFoundException("CashierShift", shiftId);

        EnsureOpen(shift);

        var expectedCash = await ComputeExpectedCashAsync(shift, cancellationToken);
        var differenceAmount = Math.Round(request.ActualCash - expectedCash, 2);

        if (differenceAmount == 0)
        {
            return await CloseInternalAsync(
                shift, request.ActualCash, expectedCash, 0, null, null,
                CashierShiftHistoryAction.ClosedBalanced, closedBy, cancellationToken);
        }

        // "Save Without Fixing": a difference reason is mandatory, and a custom description is
        // additionally mandatory when the reason is "Other". (CloseCashierShiftValidator already
        // enforces the Other -> description rule at the DTO level; whether a reason is required at
        // all depends on the computed difference above, so that part is enforced here.)
        if (request.ReasonType is null)
        {
            throw new BadRequestException("A difference reason is required because Actual Cash does not match Expected Cash.");
        }

        if (request.ReasonType == ShiftDifferenceReasonType.Other && string.IsNullOrWhiteSpace(request.ReasonDescription))
        {
            throw new BadRequestException("A custom reason description is required when reason type is 'Other'.");
        }

        return await CloseInternalAsync(
            shift, request.ActualCash, expectedCash, differenceAmount, request.ReasonType, request.ReasonDescription,
            CashierShiftHistoryAction.ClosedWithDifference, closedBy, cancellationToken);
    }

    /// <summary>
    /// Expected Cash = Opening Cash + this cashier's completed Cash sales at this branch since
    /// the shift opened - this cashier's expenses at this branch over the same period. Recomputed
    /// from scratch on every Recalculate/Close attempt, which is what makes "Fix & Recalculate"
    /// work: fixing/adding the missing invoice or expenditure elsewhere is enough, the next
    /// attempt here picks it up automatically.
    /// </summary>
    private async Task<decimal> ComputeExpectedCashAsync(CashierShift shift, CancellationToken cancellationToken)
    {
        var fromDate = shift.OpenedAt;
        var toDate = DateTime.UtcNow;

        var cashSalesTotal = await _unitOfWork.Payments.GetCashSalesTotalAsync(
            shift.BranchCode!, shift.CashierCode!, fromDate, toDate, cancellationToken);

        // Expense.ExpenseDate is a DateOnly (day granularity), so cash expenses are matched to the
        // calendar dates the shift spans rather than to its exact open/close timestamps.
        var expenses = await _unitOfWork.Expenses.SearchAsync(
            shift.BranchCode, categoryId: null, paidBy: shift.CashierCode,
            fromDate: DateOnly.FromDateTime(fromDate), toDate: DateOnly.FromDateTime(toDate),
            cancellationToken);
        var cashExpensesTotal = expenses.Sum(e => e.Amount ?? 0);

        return Math.Round(shift.OpeningCash + cashSalesTotal - cashExpensesTotal, 2);
    }

    private async Task<CashierShiftDto> CloseInternalAsync(
        CashierShift shift,
        decimal actualCash,
        decimal expectedCash,
        decimal differenceAmount,
        ShiftDifferenceReasonType? reasonType,
        string? reasonDescription,
        CashierShiftHistoryAction action,
        string closedBy,
        CancellationToken cancellationToken)
    {
        shift.ExpectedCash = expectedCash;
        shift.ActualCash = actualCash;
        shift.DifferenceAmount = differenceAmount;
        shift.ReasonType = reasonType;
        shift.ReasonDescription = reasonDescription;
        shift.Status = CashierShiftStatus.Closed;
        shift.ClosedBy = closedBy;
        shift.ClosedAt = DateTime.UtcNow;

        _unitOfWork.CashierShifts.Update(shift);

        await _unitOfWork.CashierShiftHistories.AddAsync(new CashierShiftHistory
        {
            ShiftId = shift.ShiftId,
            Action = action,
            ExpectedCash = expectedCash,
            ActualCash = actualCash,
            DifferenceAmount = differenceAmount,
            ReasonType = reasonType,
            ReasonDescription = reasonDescription,
            ChangedBy = closedBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = action == CashierShiftHistoryAction.ClosedBalanced
                ? "Shift closed - Actual Cash matched Expected Cash."
                : $"Shift closed with {(differenceAmount > 0 ? "an excess" : "a shortage")} of {Math.Abs(differenceAmount):N2} ({reasonType})."
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cashier shift {ShiftId} closed by {ClosedBy}: {Action}, expected {ExpectedCash:N2}, actual {ActualCash:N2}, difference {DifferenceAmount:N2}",
            shift.ShiftId, closedBy, action, expectedCash, actualCash, differenceAmount);

        return await GetByIdAsync(shift.ShiftId, cancellationToken);
    }

    private static void EnsureOpen(CashierShift shift)
    {
        if (shift.Status != CashierShiftStatus.Open)
        {
            throw new ConflictException($"Cashier shift '{shift.ShiftId}' is already Closed.");
        }
    }
}
