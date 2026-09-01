using PosApi.DTOs.Cash;
using PosApi.Models.Enums;

namespace PosApi.Service.Interfaces;

public interface ICashierShiftService
{
    Task<IReadOnlyList<CashierShiftDto>> SearchAsync(
        string? branchCode,
        string? cashierCode,
        CashierShiftStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<CashierShiftDto> GetByIdAsync(int shiftId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CashierShiftHistoryDto>> GetHistoryAsync(int shiftId, CancellationToken cancellationToken = default);

    /// <summary>Starts a new Day/Shift for the currently authenticated cashier. Saves Opening Cash.</summary>
    Task<CashierShiftDto> OpenShiftAsync(OpenCashierShiftDto request, string cashierCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// "Fix &amp; Recalculate": recomputes Expected Cash and compares it against the freshly
    /// entered Actual Cash. Closes the shift automatically if now balanced; otherwise records the
    /// new snapshot and keeps the shift Open.
    /// </summary>
    Task<CashierShiftDto> RecalculateAsync(int shiftId, RecalculateCashierShiftDto request, string performedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the Day/Shift. Closes normally when Expected Cash equals Actual Cash. Otherwise
    /// ("Save Without Fixing") a difference ReasonType is mandatory (ReasonDescription is
    /// additionally mandatory when ReasonType is Other).
    /// </summary>
    Task<CashierShiftDto> CloseAsync(int shiftId, CloseCashierShiftDto request, string closedBy, CancellationToken cancellationToken = default);
}
