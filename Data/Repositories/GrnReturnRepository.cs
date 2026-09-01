using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class GrnReturnRepository : GenericRepository<GrnReturn, int>, IGrnReturnRepository
{
    public GrnReturnRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<GrnReturn?> GetByIdWithDetailsAsync(int grnReturnId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.GrnMaster)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.GrnReturnId == grnReturnId, cancellationToken);
    }

    public async Task<IReadOnlyList<GrnReturn>> SearchAsync(
        int? grnId,
        GrnReturnStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(r => r.GrnMaster).AsQueryable();

        if (grnId.HasValue)
        {
            query = query.Where(r => r.GrnId == grnId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.ReturnDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.ReturnDate <= toDate.Value);
        }

        return await query.OrderByDescending(r => r.ReturnDate).ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetReturnedQuantityForGrnItemAsync(int grnItemId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<GrnReturnItem>()
            .AsNoTracking()
            .Where(i => i.GrnItemId == grnItemId)
            .SumAsync(i => i.Quantity ?? 0, cancellationToken);
    }
}
