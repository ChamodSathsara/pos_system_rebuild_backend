namespace PosApi.DTOs.Expense;

/// <summary>
/// Records a new expense for a branch. PaidBy is not accepted here - it is always set to the
/// currently authenticated user recording the expense, the same way Payment.ReceivedBy works.
/// </summary>
public class CreateExpenseDto
{
    public string BranchCode { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly? ExpenseDate { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Updates an expense's editable fields. PaidBy and CreatedAt are immutable audit fields set at
/// creation time and cannot be changed here.
/// </summary>
public class UpdateExpenseDto
{
    public string BranchCode { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly? ExpenseDate { get; set; }
    public string? Description { get; set; }
}

public class ExpenseDto
{
    public int ExpenseId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal? Amount { get; set; }
    public DateOnly? ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? PaidBy { get; set; }
    public string? PaidByName { get; set; }
    public DateTime? CreatedAt { get; set; }
}
