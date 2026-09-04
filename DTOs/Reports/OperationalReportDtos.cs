using PosApi.Models.Enums;

namespace PosApi.DTOs.Reports;

// ========================================================
// Current Stock Report
// ========================================================

public class CurrentStockReportLineDto
{
    public int StockId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }

    public string BranchCode { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;

    public decimal AvailableQty { get; set; }
    public decimal? ReorderLevel { get; set; }

    public decimal AverageUnitCost { get; set; }
    public decimal StockValue { get; set; }

    public bool IsBelowReorderLevel { get; set; }
}

public class CurrentStockReportDto
{
    public DateTime GeneratedAt { get; set; }
    public string? BranchCode { get; set; }

    public List<CurrentStockReportLineDto> Items { get; set; } = new();

    public decimal TotalQuantity { get; set; }
    public decimal TotalStockValue { get; set; }
}

// ========================================================
// Stock Movement Report
// ========================================================

public class StockMovementReportLineDto
{
    public long MovementId { get; set; }
    public DateTime CreatedAt { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }

    public string BranchCode { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;

    public long BatchId { get; set; }
    public string? BatchNo { get; set; }

    public StockMovementType MovementType { get; set; }
    public StockReferenceType ReferenceType { get; set; }

    public string? ReferenceNo { get; set; }

    public decimal Quantity { get; set; }
    public decimal PreviousQty { get; set; }
    public decimal NewQty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal MovementValue { get; set; }

    public string? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public string? Remarks { get; set; }
}

public class StockMovementReportDto
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? BranchCode { get; set; }

    public List<StockMovementReportLineDto> Movements { get; set; } = new();

    public decimal TotalInQty { get; set; }
    public decimal TotalOutQty { get; set; }
    public decimal TotalInValue { get; set; }
    public decimal TotalOutValue { get; set; }
}

// ========================================================
// Purchase / GRN Report
// ========================================================

public class PurchaseReportLineDto
{
    public string PoNo { get; set; } = string.Empty;
    public DateTime? PoDate { get; set; }
    public PurchaseOrderStatus PoStatus { get; set; }

    public int? VendorId { get; set; }
    public string? VendorName { get; set; }

    public string? BranchCode { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }

    public decimal OrderedQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal OutstandingQty { get; set; }
    public decimal ReturnedQty { get; set; }
    public decimal NetReceivedQty { get; set; }

    public decimal UnitCost { get; set; }
    public decimal OrderedValue { get; set; }
    public decimal ReceivedValue { get; set; }
    public decimal ReturnValue { get; set; }
    public decimal NetPurchaseValue { get; set; }
}

public class PurchaseReportDto
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? BranchCode { get; set; }

    public List<PurchaseReportLineDto> Items { get; set; } = new();

    public decimal TotalOrderedValue { get; set; }
    public decimal TotalReceivedValue { get; set; }
    public decimal TotalReturnValue { get; set; }
    public decimal TotalNetPurchaseValue { get; set; }
}

// ========================================================
// Expense Report
// ========================================================

public class ExpenseReportLineDto
{
    public int ExpenseId { get; set; }
    public DateOnly? ExpenseDate { get; set; }

    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }

    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public decimal Amount { get; set; }
    public string? Description { get; set; }

    public string? PaidBy { get; set; }
    public string? PaidByName { get; set; }
}

public class ExpenseCategorySummaryDto
{
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int ExpenseCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ExpenseReportDto
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? BranchCode { get; set; }

    public List<ExpenseReportLineDto> Expenses { get; set; } = new();

    public List<ExpenseCategorySummaryDto> CategorySummary { get; set; }
        = new();

    public int TotalExpenseCount { get; set; }
    public decimal TotalExpenseAmount { get; set; }
}

// ========================================================
// Profit Report
// ========================================================

public class ProfitReportDto
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? BranchCode { get; set; }

    public decimal GrossSalesExcludingTax { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal SalesReturnTotal { get; set; }
    public decimal NetRevenue { get; set; }

    public decimal SoldCost { get; set; }
    public decimal ReturnedCost { get; set; }
    public decimal NetCostOfGoodsSold { get; set; }

    public decimal GrossProfit { get; set; }
    public decimal ExpenseTotal { get; set; }
    public decimal NetProfit { get; set; }

    public decimal GrossProfitMarginPercentage { get; set; }
}

// ========================================================
// Damaged Items Report
// ========================================================

public class DamageItemReportLineDto
{
    public int DamageId { get; set; }
    public DateTime? DamageDate { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostAmount { get; set; }
    public string? Reason { get; set; }
    public string? ReportedBy { get; set; }
    public string? ReportedByName { get; set; }
    public DamageItemStatus Status { get; set; }
}

public class DamageItemReportDto
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? BranchCode { get; set; }
    public List<DamageItemReportLineDto> Items { get; set; } = new();
    public int TotalDamageCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalDamageCost { get; set; }
}
