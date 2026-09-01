using PosApi.DTOs.Payment;
using PosApi.Models.Enums;

namespace PosApi.Service.Interfaces;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentDto>> SearchAsync(
        string? invoiceNo,
        PaymentMethod? method,
        PaymentStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<PaymentDto> GetByIdAsync(int paymentId, CancellationToken cancellationToken = default);

    /// <summary>Records a payment against a sale's outstanding balance and updates the sale's PaidAmount/BalanceAmount accordingly.</summary>
    Task<PaymentDto> CreateAsync(CreatePaymentDto request, string receivedBy, CancellationToken cancellationToken = default);

    /// <summary>Voids a previously recorded payment, reversing its effect on the parent sale's PaidAmount/BalanceAmount.</summary>
    Task<PaymentDto> CancelAsync(int paymentId, CancelPaymentDto request, string cancelledBy, CancellationToken cancellationToken = default);
}
