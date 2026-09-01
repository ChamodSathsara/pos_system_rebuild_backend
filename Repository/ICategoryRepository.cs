using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface ICategoryRepository : IGenericRepository<Category, int>
{
    Task<bool> HasChildCategoriesAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<bool> HasProductsAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default);
}
