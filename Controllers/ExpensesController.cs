using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Expense;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Branch expense endpoints. Expenses are recorded against a branch and an expense category;
/// PaidBy is always set to the currently authenticated user recording the expense.
/// </summary>
[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesController : BaseApiController
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    /// <summary>Searches expenses, optionally filtered by branch, category, paid-by user, or expense date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ExpenseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? branchCode,
        [FromQuery] int? categoryId,
        [FromQuery] string? paidBy,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var expenses = await _expenseService.SearchAsync(branchCode, categoryId, paidBy, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ExpenseDto>>.SuccessResponse(expenses));
    }

    /// <summary>Retrieves a single expense.</summary>
    [HttpGet("{expenseId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int expenseId, CancellationToken cancellationToken)
    {
        var expense = await _expenseService.GetByIdAsync(expenseId, cancellationToken);
        return Ok(ApiResponse<ExpenseDto>.SuccessResponse(expense));
    }

    /// <summary>Records a new expense for a branch. PaidBy is set to the currently authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto request, CancellationToken cancellationToken)
    {
        var expense = await _expenseService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { expenseId = expense.ExpenseId },
            ApiResponse<ExpenseDto>.SuccessResponse(expense, "Expense recorded successfully."));
    }

    /// <summary>Updates an expense's branch, category, amount, date, and description. PaidBy is immutable.</summary>
    [HttpPut("{expenseId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int expenseId, [FromBody] UpdateExpenseDto request, CancellationToken cancellationToken)
    {
        var expense = await _expenseService.UpdateAsync(expenseId, request, cancellationToken);
        return Ok(ApiResponse<ExpenseDto>.SuccessResponse(expense, "Expense updated successfully."));
    }

    /// <summary>Deletes an expense record.</summary>
    [HttpDelete("{expenseId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int expenseId, CancellationToken cancellationToken)
    {
        await _expenseService.DeleteAsync(expenseId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Expense deleted successfully."));
    }
}
