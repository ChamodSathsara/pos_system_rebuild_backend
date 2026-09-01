using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IProductRepository : IGenericRepository<ProductMaster, string>
{
    Task<bool> ItemCodeExistsAsync(string itemCode, CancellationToken cancellationToken = default);

    Task<bool> BarcodeExistsAsync(string barcode, string? excludeItemCode = null, CancellationToken cancellationToken = default);

    Task<bool> HasStockAsync(string itemCode, CancellationToken cancellationToken = default);

    /// <summary>Returns the next sequential item code (e.g. "ITM00001", "ITM00002", ...) for use when the caller does not supply one explicitly.</summary>
    Task<string> GenerateNextItemCodeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductMaster>> SearchAsync(
        int? categoryId,
        int? brandId,
        bool? isActive,
        string? keyword,
        CancellationToken cancellationToken = default);

    Task<ProductMaster?> GetByIdWithDetailsAsync(string itemCode, CancellationToken cancellationToken = default);
}
