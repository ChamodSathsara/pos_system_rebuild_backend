using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IBrandRepository : IGenericRepository<Brand, int>
{
    Task<bool> HasProductsAsync(int brandId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Brand>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default);
}
