using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IPurchaseOrderItemRepository : IGenericRepository<PurchaseOrderItem, int>
{
    Task<IReadOnlyList<PurchaseOrderItem>> GetByPoNoAsync(string poNo, CancellationToken cancellationToken = default);
}