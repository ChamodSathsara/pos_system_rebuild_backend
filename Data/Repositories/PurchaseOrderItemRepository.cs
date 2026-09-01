using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class PurchaseOrderItemRepository : GenericRepository<PurchaseOrderItem, int>, IPurchaseOrderItemRepository
{
    public PurchaseOrderItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PurchaseOrderItem>> GetByPoNoAsync(string poNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.PoNo == poNo)
            .ToListAsync(cancellationToken);
    }
}