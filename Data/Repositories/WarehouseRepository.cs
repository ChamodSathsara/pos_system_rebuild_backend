using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class WarehouseRepository : GenericRepository<Warehouse, string>, IWarehouseRepository
{
    public WarehouseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> WarehouseCodeExistsAsync(string warehouseCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(w => w.WarehouseCode == warehouseCode, cancellationToken);
    }

    public async Task<IReadOnlyList<Warehouse>> GetByBranchCodeAsync(string branchCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(w => w.BranchCode == branchCode)
            .ToListAsync(cancellationToken);
    }
}
