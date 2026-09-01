using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class StockMovementRepository : GenericRepository<StockMovement, long>, IStockMovementRepository
{
    public StockMovementRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<StockMovement>> SearchAsync(
        int? stockId,
        long? batchId,
        string? referenceNo,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (stockId.HasValue)
        {
            query = query.Where(m => m.StockId == stockId.Value);
        }

        if (batchId.HasValue)
        {
            query = query.Where(m => m.BatchId == batchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(referenceNo))
        {
            query = query.Where(m => m.ReferenceNo == referenceNo);
        }

        return await query.OrderByDescending(m => m.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<StockMovement?> GetLatestForBatchAsync(long batchId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.BatchId == batchId)
            .OrderByDescending(m => m.MovementId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
