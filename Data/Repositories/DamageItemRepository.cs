using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class DamageItemRepository : GenericRepository<DamageItem, int>, IDamageItemRepository
{
    public DamageItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DamageItem?> GetByIdWithDetailsAsync(int damageId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Product)
            .Include(d => d.Branch)
            .Include(d => d.Warehouse)
            .Include(d => d.ReportedByUser)
            .FirstOrDefaultAsync(d => d.DamageId == damageId, cancellationToken);
    }

    public async Task<IReadOnlyList<DamageItem>> SearchAsync(
        string? itemCode,
        string? branchCode,
        string? warehouseCode,
        DamageItemStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(d => d.Product)
            .Include(d => d.Branch)
            .Include(d => d.Warehouse)
            .Include(d => d.ReportedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(d => d.ItemCode == itemCode);
        }

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(d => d.BranchCode == branchCode);
        }

        if (!string.IsNullOrWhiteSpace(warehouseCode))
        {
            query = query.Where(d => d.WarehouseCode == warehouseCode);
        }

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(d => d.DamageDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(d => d.DamageDate <= toDate.Value);
        }

        return await query.OrderByDescending(d => d.DamageDate).ToListAsync(cancellationToken);
    }
}
