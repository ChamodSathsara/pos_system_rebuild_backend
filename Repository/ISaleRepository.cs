using PosApi.Models.Entities;
using PosApi.Models.Enums;

namespace PosApi.Repository;

public interface ISaleRepository : IGenericRepository<Sale, string>
{
    Task<bool> InvoiceNoExistsAsync(string invoiceNo, CancellationToken cancellationToken = default);

    /// <summary>Returns the next sequential invoice number (e.g. "INV000001", "INV000002", ...) for use when the caller does not supply one explicitly.</summary>
    Task<string> GenerateNextInvoiceNoAsync(CancellationToken cancellationToken = default);

    Task<Sale?> GetByIdWithDetailsAsync(string invoiceNo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sale>> SearchAsync(
        string? branchCode,
        string? customerCode,
        SaleStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<bool> HasReturnsAsync(string invoiceNo, CancellationToken cancellationToken = default);
}
