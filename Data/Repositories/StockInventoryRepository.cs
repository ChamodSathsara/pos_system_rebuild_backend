using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class StockInventoryRepository : GenericRepository<StockInventory, int>, IStockInventoryRepository
{
    public StockInventoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<StockInventory?> GetByCombinationAsync(string itemCode, string branchCode, string warehouseCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            s => s.ItemCode == itemCode && s.BranchCode == branchCode && s.WarehouseCode == warehouseCode,
            cancellationToken);
    }

    public async Task<StockInventory?> GetByIdWithBatchesAsync(int stockId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Batches)
            .FirstOrDefaultAsync(s => s.StockId == stockId, cancellationToken);
    }

    public async Task<IReadOnlyList<StockInventory>> SearchAsync(
        string? itemCode,
        string? branchCode,
        string? warehouseCode,
        bool onlyBelowReorderLevel,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(s => s.Product).AsQueryable();

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(s => s.ItemCode == itemCode);
        }

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(s => s.BranchCode == branchCode);
        }

        if (!string.IsNullOrWhiteSpace(warehouseCode))
        {
            query = query.Where(s => s.WarehouseCode == warehouseCode);
        }

        if (onlyBelowReorderLevel)
        {
            query = query.Where(s => s.Product != null && s.Product.ReorderLevel != null && s.CurrentQty <= s.Product.ReorderLevel);
        }

        return await query.OrderBy(s => s.ItemCode).ToListAsync(cancellationToken);
    }
}
