using PosApi.Models.Entities;

namespace PosApi.Repository;

/// <summary>
/// Read-only queries over the existing Sale / SaleReturn aggregates, shaped for the Sales
/// Reports feature. Deliberately separate from <see cref="ISaleRepository"/> and
/// <see cref="ISaleReturnRepository"/> (which serve the transactional POS endpoints) since
/// reporting needs wider, filter-driven reads with different includes; it does not duplicate or
/// alter any posting/void business logic.
/// </summary>
public interface ISalesReportRepository
{
    /// <summary>
    /// Sales (with line items/products, payments, branch, customer and cashier) posted between
    /// fromDate and toDate (inclusive), matching the supplied filters. Cancelled sales are
    /// excluded since their stock was reversed and they never represented realised revenue.
    /// </summary>
    Task<IReadOnlyList<Sale>> GetSalesForReportAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        string? itemCode,
        int? categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sale returns (with line items/products and the originating sale for branch/customer info)
    /// processed between fromDate and toDate (inclusive), matching the supplied filters.
    /// </summary>
    Task<IReadOnlyList<SaleReturn>> GetSaleReturnsForReportAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        string? itemCode,
        int? categoryId,
        CancellationToken cancellationToken = default);
}
