using PosApi.DTOs.Expense;

namespace PosApi.Service.Interfaces;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> SearchAsync(
        string? branchCode,
        int? categoryId,
        string? paidBy,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default);

    Task<ExpenseDto> GetByIdAsync(int expenseId, CancellationToken cancellationToken = default);

    /// <summary>Records a new expense. PaidBy is always set to the currently authenticated user.</summary>
    Task<ExpenseDto> CreateAsync(CreateExpenseDto request, string paidBy, CancellationToken cancellationToken = default);

    Task<ExpenseDto> UpdateAsync(int expenseId, UpdateExpenseDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int expenseId, CancellationToken cancellationToken = default);
}
