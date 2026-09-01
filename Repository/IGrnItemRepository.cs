using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IGrnItemRepository : IGenericRepository<GrnItem, int>
{
    Task<IReadOnlyList<GrnItem>> GetByGrnIdAsync(int grnId, CancellationToken cancellationToken = default);

    Task<GrnItem?> GetByIdWithDetailsAsync(int grnItemId, CancellationToken cancellationToken = default);
}
