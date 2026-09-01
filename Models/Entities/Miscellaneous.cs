using PosApi.Models.Enums;

namespace PosApi.Models.Entities;

public class DamageItem
{
    public int DamageId { get; set; }
    public string? ItemCode { get; set; }
    public string? BranchCode { get; set; }
    public string? WarehouseCode { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? CostAmount { get; set; }
    public string? Reason { get; set; }
    public DateTime? DamageDate { get; set; }
    public string? ReportedBy { get; set; }
    public DamageItemStatus Status { get; set; }

    public ProductMaster? Product { get; set; }
    public Branch? Branch { get; set; }
    public Warehouse? Warehouse { get; set; }
    public SystemUser? ReportedByUser { get; set; }
}

public class Discount
{
    public string DiscountCode { get; set; } = null!;
    public string DiscountName { get; set; } = null!;
    public DiscountType DiscountType { get; set; }
    public DiscountMethod DiscountMethod { get; set; }
    public decimal? DiscountValue { get; set; }
    public string? ItemCode { get; set; }
    public decimal? MinQuantity { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public decimal? MinBillAmount { get; set; }
    public DiscountApplicableTo ApplicableTo { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ProductMaster? Product { get; set; }
    public SystemUser? CreatedByUser { get; set; }
}

public class Sale
{
    public string InvoiceNo { get; set; } = null!;
    public string? BranchCode { get; set; }
    public string? CustomerCode { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? BalanceAmount { get; set; }
    public SaleStatus Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }

    public Branch? Branch { get; set; }
    public Customer? Customer { get; set; }
    public SystemUser? CreatedByUser { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<SaleReturn> Returns { get; set; } = new List<SaleReturn>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class SaleItem
{
    public int Id { get; set; }
    public string? InvoiceNo { get; set; }
    public string? ItemCode { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalPrice { get; set; }

    public Sale? Sale { get; set; }
    public ProductMaster? Product { get; set; }
}

public class SaleReturn
{
    public string ReturnNo { get; set; } = null!;
    public string? InvoiceNo { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Reason { get; set; }
    public decimal? TotalReturnAmount { get; set; }
    public string? CreatedBy { get; set; }

    public Sale? Sale { get; set; }
    public SystemUser? CreatedByUser { get; set; }
    public ICollection<SaleReturnItem> Items { get; set; } = new List<SaleReturnItem>();
}

public class SaleReturnItem
{
    public int Id { get; set; }
    public string? ReturnNo { get; set; }
    public string? ItemCode { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalAmount { get; set; }

    public SaleReturn? SaleReturn { get; set; }
    public ProductMaster? Product { get; set; }
}

public class Payment
{
    public int PaymentId { get; set; }
    public string? InvoiceNo { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? ReferenceNo { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ReceivedBy { get; set; }

    public Sale? Sale { get; set; }
    public SystemUser? ReceivedByUser { get; set; }
}

public class ExpenseCategory
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public class Expense
{
    public int ExpenseId { get; set; }
    public string? BranchCode { get; set; }
    public int? CategoryId { get; set; }
    public decimal? Amount { get; set; }
    public DateOnly? ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? PaidBy { get; set; }
    public DateTime? CreatedAt { get; set; }

    public Branch? Branch { get; set; }
    public ExpenseCategory? Category { get; set; }
    public SystemUser? PaidByUser { get; set; }
}

/// <summary>
/// A cashier's Day/Shift cash-drawer session for a branch: opened with a counted Opening Cash
/// float and closed with a counted Actual Cash. Expected Cash is computed at close/recalculate
/// time from Opening Cash plus cash sales less cash expenses recorded by this cashier at this
/// branch since OpenedAt. DifferenceAmount is ActualCash - ExpectedCash (positive = excess,
/// negative = shortage, zero = balanced). ReasonType/ReasonDescription are only populated when
/// the shift is closed with a non-zero difference that was not fixed before closing.
/// </summary>
public class CashierShift
{
    public int ShiftId { get; set; }
    public string? BranchCode { get; set; }
    public string? CashierCode { get; set; }
    public decimal OpeningCash { get; set; }
    public DateTime OpenedAt { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? ActualCash { get; set; }
    public decimal? DifferenceAmount { get; set; }
    public ShiftDifferenceReasonType? ReasonType { get; set; }
    public string? ReasonDescription { get; set; }
    public CashierShiftStatus Status { get; set; }
    public string? ClosedBy { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Branch? Branch { get; set; }
    public SystemUser? Cashier { get; set; }
    public SystemUser? ClosedByUser { get; set; }
    public ICollection<CashierShiftHistory> Histories { get; set; } = new List<CashierShiftHistory>();
}

/// <summary>
/// Audit trail for a CashierShift: one row per Open, Recalculate attempt, and final Close
/// (balanced or with a difference), each snapshotting the Expected/Actual/Difference amounts
/// and reason (if any) at that point in time.
/// </summary>
public class CashierShiftHistory
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
    public DateTime? ChangedAt { get; set; }
    public string? Remarks { get; set; }

    public CashierShift? Shift { get; set; }
    public SystemUser? ChangedByUser { get; set; }
}

public class AuditLog
{
    public int LogId { get; set; }
    public string? UserCode { get; set; }
    public string? Action { get; set; }
    public string? TableName { get; set; }
    public string? RecordId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime? ActionTime { get; set; }
}

public class ItemLog
{
    public int LogId { get; set; }
    public string? ItemCode { get; set; }
    public string? Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime? ChangedAt { get; set; }

    public ProductMaster? Product { get; set; }
    public SystemUser? ChangedByUser { get; set; }
}
