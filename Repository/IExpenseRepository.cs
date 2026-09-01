using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IExpenseRepository : IGenericRepository<Expense, int>
{
    Task<Expense?> GetByIdWithDetailsAsync(int expenseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Expense>> SearchAsync(
        string? branchCode,
        int? categoryId,
        string? paidBy,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default);
}
