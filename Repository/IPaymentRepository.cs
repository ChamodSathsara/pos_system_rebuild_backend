using PosApi.Models.Entities;
using PosApi.Models.Enums;

namespace PosApi.Repository;

public interface IPaymentRepository : IGenericRepository<Payment, int>
{
    Task<Payment?> GetByIdWithDetailsAsync(int paymentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> SearchAsync(
        string? invoiceNo,
        PaymentMethod? method,
        PaymentStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> GetByInvoiceNoAsync(string invoiceNo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sums Completed, PaymentMethod.Cash payments received between fromDate and toDate (inclusive)
    /// for sales created by cashierCode at branchCode. Used to compute a cashier shift's Expected Cash.
    /// </summary>
    Task<decimal> GetCashSalesTotalAsync(
        string branchCode,
        string cashierCode,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}
