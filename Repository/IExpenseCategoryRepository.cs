using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IExpenseCategoryRepository : IGenericRepository<ExpenseCategory, int>
{
    Task<bool> HasExpensesAsync(int categoryId, CancellationToken cancellationToken = default);
}
