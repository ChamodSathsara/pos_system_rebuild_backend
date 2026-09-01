namespace PosApi.DTOs.Expense;

public class CreateExpenseCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateExpenseCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ExpenseCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
