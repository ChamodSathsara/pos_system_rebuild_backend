using PosApi.Models.Entities;
using PosApi.Models.Enums;

namespace PosApi.Repository;

public interface IPurchaseOrderRepository : IGenericRepository<PurchaseOrder, string>
{
    Task<bool> PoNoExistsAsync(string poNo, CancellationToken cancellationToken = default);

    /// <summary>Returns the next sequential PO number (e.g. "PO000001", "PO000002", ...) for use when the caller does not supply one explicitly.</summary>
    Task<string> GenerateNextPoNoAsync(CancellationToken cancellationToken = default);

    Task<PurchaseOrder?> GetByIdWithItemsAsync(string poNo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseOrder>> SearchAsync(
        int? vendorId,
        string? branchCode,
        PurchaseOrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<bool> HasGrnsAsync(string poNo, CancellationToken cancellationToken = default);
}