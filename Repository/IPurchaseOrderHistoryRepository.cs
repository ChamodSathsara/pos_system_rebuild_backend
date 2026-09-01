using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IPurchaseOrderHistoryRepository : IGenericRepository<PurchaseOrderHistory, int>
{
    Task<IReadOnlyList<PurchaseOrderHistory>> GetByPoNoAsync(string poNo, CancellationToken cancellationToken = default);

    Task<PurchaseOrderHistory?> GetByIdWithChangesAsync(int historyId, CancellationToken cancellationToken = default);
}