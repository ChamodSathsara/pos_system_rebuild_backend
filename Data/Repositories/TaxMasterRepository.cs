using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class TaxMasterRepository : GenericRepository<TaxMaster, string>, ITaxMasterRepository
{
    public TaxMasterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> TaxCodeExistsAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(t => t.TaxCode == taxCode, cancellationToken);
    }

    public async Task<bool> HasProductsAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ProductMaster>().AsNoTracking().AnyAsync(p => p.TaxCode == taxCode, cancellationToken);
    }

    public async Task<IReadOnlyList<TaxMaster>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        return await query.OrderBy(t => t.TaxCode).ToListAsync(cancellationToken);
    }
}
