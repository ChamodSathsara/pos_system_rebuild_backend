using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Expense;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>Expense category endpoints, used to classify branch expenses for reporting.</summary>
[ApiController]
[Route("api/expense-categories")]
[Authorize]
public class ExpenseCategoriesController : BaseApiController
{
    private readonly IExpenseCategoryService _expenseCategoryService;

    public ExpenseCategoriesController(IExpenseCategoryService expenseCategoryService)
    {
        _expenseCategoryService = expenseCategoryService;
    }

    /// <summary>Retrieves all expense categories.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ExpenseCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _expenseCategoryService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ExpenseCategoryDto>>.SuccessResponse(categories));
    }

    /// <summary>Retrieves a single expense category by id.</summary>
    [HttpGet("{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _expenseCategoryService.GetByIdAsync(categoryId, cancellationToken);
        return Ok(ApiResponse<ExpenseCategoryDto>.SuccessResponse(category));
    }

    /// <summary>Creates a new expense category.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpenseCategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseCategoryDto request, CancellationToken cancellationToken)
    {
        var category = await _expenseCategoryService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { categoryId = category.CategoryId },
            ApiResponse<ExpenseCategoryDto>.SuccessResponse(category, "Expense category created successfully."));
    }

    /// <summary>Updates an expense category.</summary>
    [HttpPut("{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int categoryId, [FromBody] UpdateExpenseCategoryDto request, CancellationToken cancellationToken)
    {
        var category = await _expenseCategoryService.UpdateAsync(categoryId, request, cancellationToken);
        return Ok(ApiResponse<ExpenseCategoryDto>.SuccessResponse(category, "Expense category updated successfully."));
    }

    /// <summary>Deletes an expense category. Fails if any expenses reference it.</summary>
    [HttpDelete("{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int categoryId, CancellationToken cancellationToken)
    {
        await _expenseCategoryService.DeleteAsync(categoryId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Expense category deleted successfully."));
    }
}
