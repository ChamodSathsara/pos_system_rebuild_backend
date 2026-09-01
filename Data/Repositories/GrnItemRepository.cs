using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class GrnItemRepository : GenericRepository<GrnItem, int>, IGrnItemRepository
{
    public GrnItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<GrnItem>> GetByGrnIdAsync(int grnId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.GrnId == grnId)
            .ToListAsync(cancellationToken);
    }

    public async Task<GrnItem?> GetByIdWithDetailsAsync(int grnItemId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.GrnMaster)
            .FirstOrDefaultAsync(i => i.GrnItemId == grnItemId, cancellationToken);
    }
}
