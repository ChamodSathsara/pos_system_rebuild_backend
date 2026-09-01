using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface ISaleReturnItemRepository : IGenericRepository<SaleReturnItem, int>
{
    Task<IReadOnlyList<SaleReturnItem>> GetByReturnNoAsync(string returnNo, CancellationToken cancellationToken = default);

    Task<SaleReturnItem?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
