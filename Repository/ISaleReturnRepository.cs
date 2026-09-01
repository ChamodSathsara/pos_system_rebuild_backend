using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface ISaleReturnRepository : IGenericRepository<SaleReturn, string>
{
    Task<bool> ReturnNoExistsAsync(string returnNo, CancellationToken cancellationToken = default);

    /// <summary>Returns the next sequential return number (e.g. "SRT000001", "SRT000002", ...) for use when the caller does not supply one explicitly.</summary>
    Task<string> GenerateNextReturnNoAsync(CancellationToken cancellationToken = default);

    Task<SaleReturn?> GetByIdWithDetailsAsync(string returnNo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SaleReturn>> SearchAsync(
        string? invoiceNo,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    /// <summary>Sums the quantity already returned for a given item across all SaleReturns raised against an invoice, so a new return can be checked against what's still returnable.</summary>
    Task<decimal> GetReturnedQuantityForItemAsync(string invoiceNo, string itemCode, CancellationToken cancellationToken = default);
}
