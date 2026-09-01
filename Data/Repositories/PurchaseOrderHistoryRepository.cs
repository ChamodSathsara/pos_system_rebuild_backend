using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class PurchaseOrderHistoryRepository : GenericRepository<PurchaseOrderHistory, int>, IPurchaseOrderHistoryRepository
{
    public PurchaseOrderHistoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PurchaseOrderHistory>> GetByPoNoAsync(string poNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(h => h.PoNo == poNo)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PurchaseOrderHistory?> GetByIdWithChangesAsync(int historyId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(h => h.Changes)
            .FirstOrDefaultAsync(h => h.HistoryId == historyId, cancellationToken);
    }
}