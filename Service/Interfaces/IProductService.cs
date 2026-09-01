using PosApi.DTOs.Product;

namespace PosApi.Service.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> SearchAsync(
        int? categoryId,
        int? brandId,
        bool? isActive,
        string? keyword,
        CancellationToken cancellationToken = default);

    Task<ProductDto> GetByIdAsync(string itemCode, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto request, CancellationToken cancellationToken = default);
    /// <summary>Updates a product. ChangedBy is recorded on the automatic item_log entries raised for the update (and for the price change, if CostPrice/SellingPrice moved).</summary>
    Task<ProductDto> UpdateAsync(string itemCode, UpdateProductDto request, string changedBy, CancellationToken cancellationToken = default);
    Task DeleteAsync(string itemCode, CancellationToken cancellationToken = default);
}
