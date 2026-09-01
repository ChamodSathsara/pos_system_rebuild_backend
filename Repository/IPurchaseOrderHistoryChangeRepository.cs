using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IPurchaseOrderHistoryChangeRepository : IGenericRepository<PurchaseOrderHistoryChange, int>
{
    Task<IReadOnlyList<PurchaseOrderHistoryChange>> GetByHistoryIdAsync(int historyId, CancellationToken cancellationToken = default);
}