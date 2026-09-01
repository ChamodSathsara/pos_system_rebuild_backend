using PosApi.DTOs.Product;

namespace PosApi.Service.Interfaces;

public interface IBrandService
{
    Task<IReadOnlyList<BrandDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<BrandDto> GetByIdAsync(int brandId, CancellationToken cancellationToken = default);
    Task<BrandDto> CreateAsync(CreateBrandDto request, CancellationToken cancellationToken = default);
    Task<BrandDto> UpdateAsync(int brandId, UpdateBrandDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int brandId, CancellationToken cancellationToken = default);
}
