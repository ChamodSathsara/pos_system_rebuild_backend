using PosApi.Models.Enums;

namespace PosApi.DTOs.Payment;

/// <summary>
/// Records a payment against a completed sale. The amount can never exceed the sale's current
/// BalanceAmount. Recording a payment increments the sale's PaidAmount and reduces its
/// BalanceAmount accordingly.
/// </summary>
public class CreatePaymentDto
{
    public string InvoiceNo { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? ReferenceNo { get; set; }
}

/// <summary>Cancels/voids a previously recorded payment, reversing its effect on the parent sale.</summary>
public class CancelPaymentDto
{
    public string? Remarks { get; set; }
}

public class PaymentDto
{
    public int PaymentId { get; set; }
    public string? InvoiceNo { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? ReferenceNo { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ReceivedBy { get; set; }
}
