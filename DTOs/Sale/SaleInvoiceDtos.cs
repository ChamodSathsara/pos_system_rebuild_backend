using PosApi.Models.Enums;

namespace PosApi.DTOs.Sale;

/// <summary>One priced line on a printed invoice.</summary>
public class SaleInvoiceItemDto
{
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }

    /// <summary>Original per-unit selling price, before any line discount.</summary>
    public decimal Price { get; set; }

    /// <summary>"Last Price" - the effective per-unit price after the line's discount is applied.</summary>
    public decimal Lp { get; set; }

    /// <summary>Quantity x LP (the taxable/net amount for the line, excluding tax).</summary>
    public decimal Amount { get; set; }
}

/// <summary>One payment line to print under the totals (e.g. "CASH 2000.00").</summary>
public class SaleInvoicePaymentDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
}

/// <summary>
/// Everything needed to render a printed/thermal invoice for a completed sale: company and
/// branch letterhead, cashier and customer, priced line items (Price/LP/Amt), and the bill's
/// totals and settlement. Built from the same Sale aggregate used for <see cref="SaleDto"/>.
/// </summary>
public class SaleInvoiceDto
{
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime? SaleDate { get; set; }

    // Letterhead
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }

    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? BranchAddress { get; set; }
    public string? BranchPhone { get; set; }

    public string? CashierCode { get; set; }
    public string? CashierName { get; set; }

    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }

    public List<SaleInvoiceItemDto> Items { get; set; } = new();

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }

    public List<SaleInvoicePaymentDto> Payments { get; set; } = new();
}
