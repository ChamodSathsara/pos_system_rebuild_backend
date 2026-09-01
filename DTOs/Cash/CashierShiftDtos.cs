using PosApi.Models.Enums;

namespace PosApi.DTOs.Cash;

/// <summary>
/// Starts a cashier's Day/Shift at a branch. CashierCode is not accepted here - it is always
/// set to the currently authenticated user opening the shift. A cashier cannot have more than
/// one Open shift at the same branch at a time.
/// </summary>
public class OpenCashierShiftDto
{
    public string BranchCode { get; set; } = string.Empty;
    public decimal OpeningCash { get; set; }
}

/// <summary>
/// "Fix &amp; Recalculate": re-enters the counted Actual Cash and asks the system to recompute
/// Expected Cash from scratch (picking up any missing invoice/expenditure the user has since
/// corrected). If the recomputed amounts now match, the shift is closed automatically as
/// balanced. If a difference remains, the shift stays Open with the new snapshot recorded so
/// the caller can fix further or call Close to save without fixing.
/// </summary>
public class RecalculateCashierShiftDto
{
    public decimal ActualCash { get; set; }
}

/// <summary>
/// Ends a cashier's Day/Shift. Expected Cash is (re)computed fresh from Opening Cash plus this
/// cashier's cash sales less cash expenses at this branch since the shift opened. If Expected
/// Cash equals ActualCash the shift closes normally and ReasonType/ReasonDescription are
/// ignored. Otherwise ("Save Without Fixing") ReasonType is mandatory, and ReasonDescription is
/// additionally mandatory when ReasonType is Other.
/// </summary>
public class CloseCashierShiftDto
{
    public decimal ActualCash { get; set; }
    public ShiftDifferenceReasonType? ReasonType { get; set; }
    public string? ReasonDescription { get; set; }
}

public class CashierShiftDto
{
    public int ShiftId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? CashierCode { get; set; }
    public string? CashierName { get; set; }
    public decimal OpeningCash { get; set; }
    public DateTime OpenedAt { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? ActualCash { get; set; }
    public decimal? DifferenceAmount { get; set; }
    public ShiftDifferenceReasonType? ReasonType { get; set; }
    public string? ReasonDescription { get; set; }
    public CashierShiftStatus Status { get; set; }
    public string? ClosedBy { get; set; }
    public string? ClosedByName { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class CashierShiftHistoryDto
{
    public int HistoryId { get; set; }
    public int? ShiftId { get; set; }
    public CashierShiftHistoryAction Action { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? ActualCash { get; set; }
    public decimal? DifferenceAmount { get; set; }
    public ShiftDifferenceReasonType? ReasonType { get; set; }
    public string? ReasonDescription { get; set; }
    public string? ChangedBy { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime? ChangedAt { get; set; }
    public string? Remarks { get; set; }
}
