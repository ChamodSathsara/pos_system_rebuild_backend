using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class ExpenseCategoryRepository : GenericRepository<ExpenseCategory, int>, IExpenseCategoryRepository
{
    public ExpenseCategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasExpensesAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Expense>().AsNoTracking().AnyAsync(e => e.CategoryId == categoryId, cancellationToken);
    }

    public override async Task<IReadOnlyList<ExpenseCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync(cancellationToken);
    }
}
