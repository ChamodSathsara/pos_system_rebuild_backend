using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IStockInventoryRepository : IGenericRepository<StockInventory, int>
{
    Task<StockInventory?> GetByCombinationAsync(string itemCode, string branchCode, string warehouseCode, CancellationToken cancellationToken = default);

    Task<StockInventory?> GetByIdWithBatchesAsync(int stockId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockInventory>> SearchAsync(
        string? itemCode,
        string? branchCode,
        string? warehouseCode,
        bool onlyBelowReorderLevel,
        CancellationToken cancellationToken = default);
}
