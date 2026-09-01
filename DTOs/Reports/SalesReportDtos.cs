using PosApi.Models.Enums;

namespace PosApi.DTOs.Reports;

/// <summary>One payment method's contribution to a period's collections (count + amount).</summary>
public class PaymentMethodSummaryDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

// ---------------------------------------------------------------------------------------------
// 1. Daily Sales Report
// ---------------------------------------------------------------------------------------------

/// <summary>One calendar day's figures within a Daily Sales Report.</summary>
public class DailySalesReportRowDto
{
    public DateOnly Date { get; set; }
    public int InvoiceCount { get; set; }

    /// <summary>Sum of Sale.Subtotal (qty x unit price, before discount/tax) for invoices posted this day.</summary>
    public decimal GrossSales { get; set; }

    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }

    /// <summary>Sum of SaleReturn.TotalReturnAmount for returns processed this day.</summary>
    public decimal ReturnsTotal { get; set; }

    /// <summary>(GrossSales - DiscountTotal + TaxTotal) - ReturnsTotal.</summary>
    public decimal NetSales { get; set; }

    public List<PaymentMethodSummaryDto> PaymentSummary { get; set; } = new();
}

/// <summary>
/// Day-by-day breakdown of sales activity for the requested date range (a single-day request
/// simply returns one row), plus a grand total row across the whole range.
/// </summary>
public class DailySalesReportDto
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public List<DailySalesReportRowDto> Days { get; set; } = new();
    public DailySalesReportRowDto Total { get; set; } = new();
}

// ---------------------------------------------------------------------------------------------
// 2. Sales Summary Report
// ---------------------------------------------------------------------------------------------

/// <summary>Rolled-up sales summary across an entire date range (no daily breakdown).</summary>
public class SalesSummaryReportDto
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }

    public int TotalInvoices { get; set; }
    public decimal TotalQuantitySold { get; set; }
    public decimal GrossSales { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal ReturnsTotal { get; set; }

    /// <summary>(GrossSales - DiscountTotal + TaxTotal) - ReturnsTotal.</summary>
    public decimal NetSales { get; set; }
}

// ---------------------------------------------------------------------------------------------
// 3. Item-wise Sales Report
// ---------------------------------------------------------------------------------------------

public class ItemWiseSalesReportLineDto
{
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal QuantitySold { get; set; }

    /// <summary>Sum of qty x unit price, before discount.</summary>
    public decimal SellingAmount { get; set; }

    public decimal DiscountAmount { get; set; }
    public decimal ReturnQuantity { get; set; }
    public decimal ReturnAmount { get; set; }

    /// <summary>SellingAmount - DiscountAmount - ReturnAmount.</summary>
    public decimal NetSalesAmount { get; set; }
}

public class ItemWiseSalesReportDto
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public List<ItemWiseSalesReportLineDto> Items { get; set; } = new();
    public ItemWiseSalesReportLineDto Total { get; set; } = new();
}

// ---------------------------------------------------------------------------------------------
// 4. Sales Return Report
// ---------------------------------------------------------------------------------------------

public class SalesReturnReportItemDto
{
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
}

public class SalesReturnReportLineDto
{
    public string ReturnNo { get; set; } = string.Empty;
    public DateTime? ReturnDate { get; set; }
    public string? InvoiceNo { get; set; }
    public string? BranchCode { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? Reason { get; set; }
    public string? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public decimal TotalReturnAmount { get; set; }
    public List<SalesReturnReportItemDto> Items { get; set; } = new();
}

public class SalesReturnReportDto
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public int TotalReturns { get; set; }
    public decimal TotalReturnAmount { get; set; }
    public List<SalesReturnReportLineDto> Returns { get; set; } = new();
}
