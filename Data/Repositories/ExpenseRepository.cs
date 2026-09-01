using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class ExpenseRepository : GenericRepository<Expense, int>, IExpenseRepository
{
    public ExpenseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Expense?> GetByIdWithDetailsAsync(int expenseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Branch)
            .Include(e => e.Category)
            .Include(e => e.PaidByUser)
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId, cancellationToken);
    }

    public async Task<IReadOnlyList<Expense>> SearchAsync(
        string? branchCode,
        int? categoryId,
        string? paidBy,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.Category)
            .Include(e => e.PaidByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(e => e.BranchCode == branchCode);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(paidBy))
        {
            query = query.Where(e => e.PaidBy == paidBy);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= toDate.Value);
        }

        return await query.OrderByDescending(e => e.ExpenseDate).ToListAsync(cancellationToken);
    }
}
