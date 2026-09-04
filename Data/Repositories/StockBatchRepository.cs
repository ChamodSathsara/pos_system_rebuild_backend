using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class StockBatchRepository : GenericRepository<StockBatch, long>, IStockBatchRepository
{
    public StockBatchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<StockBatch>> GetByStockIdAsync(int stockId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(b => b.StockId == stockId)
            .OrderBy(b => b.ReceivedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> BatchNoExistsAsync(int stockId, string batchNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(b => b.StockId == stockId && b.BatchNo == batchNo, cancellationToken);
    }

    public async Task<StockBatch?> GetByStockIdAndBatchNoAsync(int stockId, string batchNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(b => b.StockId == stockId && b.BatchNo == batchNo, cancellationToken);
    }

    public async Task<bool> HasMovementsBeyondReceiptAsync(long batchId, CancellationToken cancellationToken = default)
    {
        // A freshly received batch that has never been touched has AvailableQty == ReceivedQty
        // and exactly one movement (the initial receipt). Anything more means it's been consumed
        // or adjusted and can no longer be deleted outright.
        var movementCount = await Context.Set<StockMovement>().AsNoTracking().CountAsync(m => m.BatchId == batchId, cancellationToken);
        return movementCount > 1;
    }

    public async Task<IReadOnlyList<StockBatch>> GetAvailableBatchesByItemAndBranchAsync(string itemCode, string branchCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.StockInventory)
            .Where(b => b.AvailableQty > 0
                && b.Status == BatchStatus.Available
                && b.StockInventory != null
                && b.StockInventory.ItemCode == itemCode
                && b.StockInventory.BranchCode == branchCode)
            .OrderBy(b => b.ReceivedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockBatch>> GetAvailableBatchesByItemsAndBranchAsync(
        IReadOnlyCollection<string> itemCodes,
        string branchCode,
        CancellationToken cancellationToken = default)
    {
        if (itemCodes.Count == 0)
        {
            return Array.Empty<StockBatch>();
        }

        return await DbSet
            .Include(b => b.StockInventory)
            .Where(b => b.AvailableQty > 0
                && b.Status == BatchStatus.Available
                && b.StockInventory != null
                && itemCodes.Contains(b.StockInventory.ItemCode!)
                && b.StockInventory.BranchCode == branchCode)
            .OrderBy(b => b.ReceivedDate)
            .ThenBy(b => b.BatchId)
            .ToListAsync(cancellationToken);
    }
}
