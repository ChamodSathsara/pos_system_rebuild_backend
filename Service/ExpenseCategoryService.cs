using AutoMapper;
using PosApi.DTOs.Expense;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ExpenseCategoryService> _logger;

    public ExpenseCategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ExpenseCategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.ExpenseCategories.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ExpenseCategoryDto>>(categories);
    }

    public async Task<ExpenseCategoryDto> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("ExpenseCategory", categoryId);

        return _mapper.Map<ExpenseCategoryDto>(category);
    }

    public async Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = new ExpenseCategory
        {
            CategoryName = request.CategoryName.Trim(),
            Description = request.Description
        };

        await _unitOfWork.ExpenseCategories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expense category {CategoryName} created with id {CategoryId}", category.CategoryName, category.CategoryId);

        return _mapper.Map<ExpenseCategoryDto>(category);
    }

    public async Task<ExpenseCategoryDto> UpdateAsync(int categoryId, UpdateExpenseCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("ExpenseCategory", categoryId);

        category.CategoryName = request.CategoryName.Trim();
        category.Description = request.Description;

        _unitOfWork.ExpenseCategories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expense category {CategoryId} updated successfully", categoryId);

        return _mapper.Map<ExpenseCategoryDto>(category);
    }

    public async Task DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("ExpenseCategory", categoryId);

        if (await _unitOfWork.ExpenseCategories.HasExpensesAsync(categoryId, cancellationToken))
        {
            throw new ConflictException($"Expense category '{category.CategoryName}' has expenses recorded against it and cannot be deleted.");
        }

        _unitOfWork.ExpenseCategories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expense category {CategoryId} deleted successfully", categoryId);
    }
}
