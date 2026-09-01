using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class CashierShiftRepository : GenericRepository<CashierShift, int>, ICashierShiftRepository
{
    public CashierShiftRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CashierShift?> GetByIdWithDetailsAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Branch)
            .Include(s => s.Cashier)
            .Include(s => s.ClosedByUser)
            .FirstOrDefaultAsync(s => s.ShiftId == shiftId, cancellationToken);
    }

    public async Task<CashierShift?> GetOpenShiftAsync(string cashierCode, string branchCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                s => s.CashierCode == cashierCode && s.BranchCode == branchCode && s.Status == CashierShiftStatus.Open,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CashierShift>> SearchAsync(
        string? branchCode,
        string? cashierCode,
        CashierShiftStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(s => s.Branch)
            .Include(s => s.Cashier)
            .Include(s => s.ClosedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(s => s.BranchCode == branchCode);
        }

        if (!string.IsNullOrWhiteSpace(cashierCode))
        {
            query = query.Where(s => s.CashierCode == cashierCode);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.OpenedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.OpenedAt <= toDate.Value);
        }

        return await query.OrderByDescending(s => s.OpenedAt).ToListAsync(cancellationToken);
    }
}
