namespace PosApi.Models.Enums;

public enum StockReferenceType
{
    OpeningStock,
    Grn,
    Sale,
    SaleReturn,
    GrnReturn,
    StockTransfer,
    StockAdjustment,
    Damage,
    StockReceive,
    CostCorrection
}

public enum DiscountType
{
    Item,
    Item_Quantity,
    Seasonal,
    Total_Bill,
    Special
}

public enum ChequeStatus
{
    Pending,
    Realized,
    Bounced
}

public enum BranchStatus
{
    Active,
    Inactive
}

public enum Gender
{
    Male,
    Female,
    Other
}

public enum CustomerType
{
    Regular,
    Credit,
    Wholesale,
    VIP,
    Employee
}

public enum PurchaseOrderChangeField
{
    Vendor,
    ExpectedDate,
    Remarks,
    Quantity,
    UnitCost,
    TotalCost,
    Status,
    ItemStatus
}

public enum StockMovementType
{
    In,
    Out,
    Adjustment,
    CostCorrection
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded,
    Cancelled
}

public enum DamageItemStatus
{
    Reported,
    Reviewed,
    Approved,
    Disposed,
    Rejected
}

public enum UnitOfMeasure
{
    PCS,
    KG,
    LTR,
    L,
    ML,
    M,
    CM,
    PACK,
    BOX,
    DOZEN
}

public enum GrnReturnStatus
{
    Pending,
    Approved,
    Completed,
    Rejected
}

public enum BatchStatus
{
    Available,
    Completed,
    Expired,
    Damaged,
    Blocked
}

public enum DiscountMethod
{
    Percentage,
    Fixed_Amount
}

public enum DiscountApplicableTo
{
    Entire_Bill,
    Selected_Items
}

public enum SaleStatus
{
    Pending,
    Completed,
    Cancelled,
    Refunded
}

public enum PurchaseOrderHistoryAction
{
    Created,
    Modified,
    Approved,
    Rejected,
    Cancelled,
    StatusChanged
}

public enum PaymentMethod
{
    Cash,
    Card,
    BankTransfer,
    Cheque,
    Online,
    LoyaltyPoints
}

public enum ItemGroup
{
    Machinery,
    Consumables,
    Stationery,
    SpareParts,
    Services
}

public enum PurchaseOrderStatus
{
    Open,
    PartiallyReceived,
    FullyReceived,
    Cancelled
}

public enum CashierShiftStatus
{
    Open,
    Closed
}

/// <summary>
/// Why Actual Cash did not match Expected Cash at shift close. Other requires a mandatory
/// custom ReasonDescription on the shift/history record.
/// </summary>
public enum ShiftDifferenceReasonType
{
    MissingInvoice,
    MissingExpenditure,
    CashHandlingError,
    Other
}

/// <summary>
/// Audit trail action for CashierShiftHistory rows raised as a shift progresses.
/// </summary>
public enum CashierShiftHistoryAction
{
    Opened,
    Recalculated,
    ClosedBalanced,
    ClosedWithDifference
}
