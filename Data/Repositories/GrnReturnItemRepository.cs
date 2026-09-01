using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class GrnReturnItemRepository : GenericRepository<GrnReturnItem, int>, IGrnReturnItemRepository
{
    public GrnReturnItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<GrnReturnItem>> GetByGrnReturnIdAsync(int grnReturnId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.GrnReturnId == grnReturnId)
            .ToListAsync(cancellationToken);
    }

    public async Task<GrnReturnItem?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.GrnReturn)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }
}
