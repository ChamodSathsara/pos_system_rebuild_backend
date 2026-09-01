using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class ItemLogRepository : GenericRepository<ItemLog, int>, IItemLogRepository
{
    public ItemLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ItemLog?> GetByIdWithDetailsAsync(int logId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(l => l.Product)
            .Include(l => l.ChangedByUser)
            .FirstOrDefaultAsync(l => l.LogId == logId, cancellationToken);
    }

    public async Task<IReadOnlyList<ItemLog>> SearchAsync(
        string? itemCode,
        string? action,
        string? changedBy,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(l => l.Product)
            .Include(l => l.ChangedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(l => l.ItemCode == itemCode);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(l => l.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(changedBy))
        {
            query = query.Where(l => l.ChangedBy == changedBy);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.ChangedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.ChangedAt <= toDate.Value);
        }

        return await query.OrderByDescending(l => l.ChangedAt).ToListAsync(cancellationToken);
    }
}
