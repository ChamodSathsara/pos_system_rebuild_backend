using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Cash;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Cashier Day/Shift cash-drawer closing endpoints. A shift is opened with a counted Opening
/// Cash float and ended by entering the counted Actual Cash: the system computes Expected Cash
/// and compares it. A balanced count closes the shift immediately. A shortage/excess can either
/// be fixed and recalculated (via Recalculate, after correcting the missing invoice/expenditure
/// elsewhere) or saved without fixing (via Close, which then requires a difference reason).
/// CashierCode/ClosedBy are always set to the currently authenticated user.
/// </summary>
[ApiController]
[Route("api/cashier-shifts")]
[Authorize]
public class CashierShiftsController : BaseApiController
{
    private readonly ICashierShiftService _cashierShiftService;

    public CashierShiftsController(ICashierShiftService cashierShiftService)
    {
        _cashierShiftService = cashierShiftService;
    }

    /// <summary>Searches cashier shifts, optionally filtered by branch, cashier, status, or opened-date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CashierShiftDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? branchCode,
        [FromQuery] string? cashierCode,
        [FromQuery] CashierShiftStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var shifts = await _cashierShiftService.SearchAsync(branchCode, cashierCode, status, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CashierShiftDto>>.SuccessResponse(shifts));
    }

    /// <summary>Retrieves a single cashier shift.</summary>
    [HttpGet("{shiftId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int shiftId, CancellationToken cancellationToken)
    {
        var shift = await _cashierShiftService.GetByIdAsync(shiftId, cancellationToken);
        return Ok(ApiResponse<CashierShiftDto>.SuccessResponse(shift));
    }

    /// <summary>Retrieves the audit history (open, recalculate attempts, close) for a cashier shift.</summary>
    [HttpGet("{shiftId:int}/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CashierShiftHistoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(int shiftId, CancellationToken cancellationToken)
    {
        var history = await _cashierShiftService.GetHistoryAsync(shiftId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CashierShiftHistoryDto>>.SuccessResponse(history));
    }

    /// <summary>Starts a new Day/Shift for the currently authenticated cashier. Saves Opening Cash.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Open([FromBody] OpenCashierShiftDto request, CancellationToken cancellationToken)
    {
        var shift = await _cashierShiftService.OpenShiftAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { shiftId = shift.ShiftId },
            ApiResponse<CashierShiftDto>.SuccessResponse(shift, "Cashier shift opened successfully."));
    }

    /// <summary>
    /// "Fix &amp; Recalculate": re-enters Actual Cash and recomputes Expected Cash. Closes the
    /// shift automatically if now balanced; otherwise records the new snapshot and keeps it Open.
    /// </summary>
    [HttpPost("{shiftId:int}/recalculate")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Recalculate(int shiftId, [FromBody] RecalculateCashierShiftDto request, CancellationToken cancellationToken)
    {
        var shift = await _cashierShiftService.RecalculateAsync(shiftId, request, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<CashierShiftDto>.SuccessResponse(shift, "Cashier shift recalculated successfully."));
    }

    /// <summary>
    /// Ends the Day/Shift. Closes normally when Actual Cash matches Expected Cash. Otherwise
    /// ("Save Without Fixing") a difference ReasonType is mandatory, and ReasonDescription is
    /// additionally mandatory when ReasonType is Other.
    /// </summary>
    [HttpPost("{shiftId:int}/close")]
    [ProducesResponseType(typeof(ApiResponse<CashierShiftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close(int shiftId, [FromBody] CloseCashierShiftDto request, CancellationToken cancellationToken)
    {
        var shift = await _cashierShiftService.CloseAsync(shiftId, request, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<CashierShiftDto>.SuccessResponse(shift, "Cashier shift closed successfully."));
    }
}
