using PosApi.DTOs.Stock;

namespace PosApi.Service.Interfaces;

public interface IStockInventoryService
{
    Task<IReadOnlyList<StockInventoryDto>> SearchAsync(
        string? itemCode,
        string? branchCode,
        string? warehouseCode,
        bool onlyBelowReorderLevel = false,
        CancellationToken cancellationToken = default);

    Task<StockInventoryDto> GetByIdAsync(int stockId, CancellationToken cancellationToken = default);

    Task<StockInventoryDto> CreateAsync(CreateStockInventoryDto request, CancellationToken cancellationToken = default);

    /// <summary>Recounts CurrentQty from the sum of the stock line's batches and refreshes LastUpdated.</summary>
    Task<StockInventoryDto> ReconcileAsync(int stockId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int stockId, CancellationToken cancellationToken = default);
}
