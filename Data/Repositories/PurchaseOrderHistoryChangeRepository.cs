using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class PurchaseOrderHistoryChangeRepository : GenericRepository<PurchaseOrderHistoryChange, int>, IPurchaseOrderHistoryChangeRepository
{
    public PurchaseOrderHistoryChangeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PurchaseOrderHistoryChange>> GetByHistoryIdAsync(int historyId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(c => c.HistoryId == historyId)
            .ToListAsync(cancellationToken);
    }
}