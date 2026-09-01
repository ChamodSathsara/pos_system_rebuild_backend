using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class SaleReturnItemRepository : GenericRepository<SaleReturnItem, int>, ISaleReturnItemRepository
{
    public SaleReturnItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SaleReturnItem>> GetByReturnNoAsync(string returnNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.ReturnNo == returnNo)
            .ToListAsync(cancellationToken);
    }

    public async Task<SaleReturnItem?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.SaleReturn)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }
}
