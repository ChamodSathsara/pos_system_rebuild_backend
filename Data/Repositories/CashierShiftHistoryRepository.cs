using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class CashierShiftHistoryRepository : GenericRepository<CashierShiftHistory, int>, ICashierShiftHistoryRepository
{
    public CashierShiftHistoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CashierShiftHistory>> GetByShiftIdAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(h => h.ChangedByUser)
            .Where(h => h.ShiftId == shiftId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync(cancellationToken);
    }
}
