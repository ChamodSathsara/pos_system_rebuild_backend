using PosApi.DTOs.Product;

namespace PosApi.Service.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<CategoryDto> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryDto request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(int categoryId, UpdateCategoryDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int categoryId, CancellationToken cancellationToken = default);
}
