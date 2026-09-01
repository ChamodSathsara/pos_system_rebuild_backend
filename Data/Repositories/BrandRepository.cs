using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class BrandRepository : GenericRepository<Brand, int>, IBrandRepository
{
    public BrandRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasProductsAsync(int brandId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ProductMaster>().AsNoTracking().AnyAsync(p => p.BrandId == brandId, cancellationToken);
    }

    public async Task<IReadOnlyList<Brand>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(b => b.IsActive == isActive.Value);
        }

        return await query.OrderBy(b => b.BrandName).ToListAsync(cancellationToken);
    }
}
