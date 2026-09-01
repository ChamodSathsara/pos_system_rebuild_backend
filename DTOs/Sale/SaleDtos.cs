using PosApi.Models.Enums;

namespace PosApi.DTOs.Sale;

public class CreateSaleItemLineDto
{
    public string ItemCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    /// <summary>Optional. Falls back to the product's current SellingPrice when omitted.</summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>Optional line-level discount. Defaults to 0.</summary>
    public decimal? DiscountAmount { get; set; }
}

/// <summary>
/// Posts a completed POS sale with its line items in one transaction. Stock is drawn down
/// immediately (FIFO across the branch's available batches) since a Sale represents goods that
/// have already left the store - there is no separate "confirm" step. Optional initial payments
/// can be supplied so the sale is recorded as paid/part-paid from the moment it's created; more
/// payments can be added afterwards via PaymentsController.
/// </summary>
public class CreateSaleDto
{
    /// <summary>Optional. Auto-generated (e.g. INV000001) when omitted.</summary>
    public string? InvoiceNo { get; set; }
    public string BranchCode { get; set; } = string.Empty;

    /// <summary>Optional - omit for a walk-in/cash customer.</summary>
    public string? CustomerCode { get; set; }
    public DateTime? SaleDate { get; set; }

    /// <summary>Optional bill-level discount, applied on top of any line-level discounts.</summary>
    public decimal? DiscountAmount { get; set; }
    public List<CreateSaleItemLineDto> Items { get; set; } = new();
    public List<CreateSalePaymentLineDto> Payments { get; set; } = new();
}

/// <summary>An initial payment recorded at the moment a sale is posted.</summary>
public class CreateSalePaymentLineDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
}

public class SaleItemDto
{
    public int Id { get; set; }
    public string? InvoiceNo { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TotalPrice { get; set; }
}

public class SaleDto
{
    public string InvoiceNo { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
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
    public List<SaleItemDto> Items { get; set; } = new();
}
