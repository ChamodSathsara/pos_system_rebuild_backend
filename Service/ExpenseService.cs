using AutoMapper;
using PosApi.DTOs.Expense;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ExpenseService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExpenseDto>> SearchAsync(
        string? branchCode,
        int? categoryId,
        string? paidBy,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var expenses = await _unitOfWork.Expenses.SearchAsync(branchCode, categoryId, paidBy, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<ExpenseDto>>(expenses);
    }

    public async Task<ExpenseDto> GetByIdAsync(int expenseId, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(expenseId, cancellationToken)
            ?? throw new NotFoundException("Expense", expenseId);

        return _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseDto request, string paidBy, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new BadRequestException("Expense amount must be greater than zero.");
        }

        var branch = await _unitOfWork.Branches.GetByIdAsync(request.BranchCode, cancellationToken)
            ?? throw new NotFoundException("Branch", request.BranchCode);

        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("ExpenseCategory", request.CategoryId);

        var expense = new Expense
        {
            BranchCode = branch.BranchCode,
            CategoryId = category.CategoryId,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Description = request.Description,
            PaidBy = paidBy,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Expenses.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Expense {ExpenseId} of {Amount:N2} recorded for branch {BranchCode} in category {CategoryId} by {PaidBy}",
            expense.ExpenseId, request.Amount, branch.BranchCode, category.CategoryId, paidBy);

        return await GetByIdAsync(expense.ExpenseId, cancellationToken);
    }

    public async Task<ExpenseDto> UpdateAsync(int expenseId, UpdateExpenseDto request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new BadRequestException("Expense amount must be greater than zero.");
        }

        var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId, cancellationToken)
            ?? throw new NotFoundException("Expense", expenseId);

        var branch = await _unitOfWork.Branches.GetByIdAsync(request.BranchCode, cancellationToken)
            ?? throw new NotFoundException("Branch", request.BranchCode);

        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("ExpenseCategory", request.CategoryId);

        expense.BranchCode = branch.BranchCode;
        expense.CategoryId = category.CategoryId;
        expense.Amount = request.Amount;
        expense.ExpenseDate = request.ExpenseDate ?? expense.ExpenseDate;
        expense.Description = request.Description;

        _unitOfWork.Expenses.Update(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expense {ExpenseId} updated successfully", expenseId);

        return await GetByIdAsync(expenseId, cancellationToken);
    }

    public async Task DeleteAsync(int expenseId, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId, cancellationToken)
            ?? throw new NotFoundException("Expense", expenseId);

        _unitOfWork.Expenses.Remove(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expense {ExpenseId} deleted successfully", expenseId);
    }
}
