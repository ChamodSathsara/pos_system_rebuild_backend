using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IStockMovementRepository : IGenericRepository<StockMovement, long>
{
    Task<IReadOnlyList<StockMovement>> SearchAsync(
        int? stockId,
        long? batchId,
        string? referenceNo,
        CancellationToken cancellationToken = default);

    Task<StockMovement?> GetLatestForBatchAsync(long batchId, CancellationToken cancellationToken = default);
}
