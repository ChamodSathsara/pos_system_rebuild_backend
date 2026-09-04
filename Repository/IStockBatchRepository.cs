using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IStockBatchRepository : IGenericRepository<StockBatch, long>
{
    Task<IReadOnlyList<StockBatch>> GetByStockIdAsync(int stockId, CancellationToken cancellationToken = default);

    Task<bool> BatchNoExistsAsync(int stockId, string batchNo, CancellationToken cancellationToken = default);

    Task<StockBatch?> GetByStockIdAndBatchNoAsync(int stockId, string batchNo, CancellationToken cancellationToken = default);

    Task<bool> HasMovementsBeyondReceiptAsync(long batchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracked, oldest-first list of batches with available stock for an item across every
    /// warehouse in a branch (Sale only carries a BranchCode, not a warehouse). Used to draw
    /// down stock FIFO when posting a sale.
    /// </summary>
    Task<IReadOnlyList<StockBatch>> GetAvailableBatchesByItemAndBranchAsync(string itemCode, string branchCode, CancellationToken cancellationToken = default);

    /// <summary>Loads tracked FIFO batches for every requested item in one database query.</summary>
    Task<IReadOnlyList<StockBatch>> GetAvailableBatchesByItemsAndBranchAsync(
        IReadOnlyCollection<string> itemCodes,
        string branchCode,
        CancellationToken cancellationToken = default);
}
