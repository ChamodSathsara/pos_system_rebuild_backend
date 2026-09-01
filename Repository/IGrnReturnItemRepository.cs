using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IGrnReturnItemRepository : IGenericRepository<GrnReturnItem, int>
{
    Task<IReadOnlyList<GrnReturnItem>> GetByGrnReturnIdAsync(int grnReturnId, CancellationToken cancellationToken = default);

    Task<GrnReturnItem?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
