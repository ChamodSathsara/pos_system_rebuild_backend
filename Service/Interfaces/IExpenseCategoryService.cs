using PosApi.DTOs.Expense;

namespace PosApi.Service.Interfaces;

public interface IExpenseCategoryService
{
    Task<IReadOnlyList<ExpenseCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExpenseCategoryDto> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto request, CancellationToken cancellationToken = default);
    Task<ExpenseCategoryDto> UpdateAsync(int categoryId, UpdateExpenseCategoryDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int categoryId, CancellationToken cancellationToken = default);
}
