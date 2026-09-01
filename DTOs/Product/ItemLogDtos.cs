namespace PosApi.DTOs.Product;

/// <summary>
/// Records a new item change-log entry. ChangedBy is not accepted here - it is always set to
/// the currently authenticated user, the same way Expense.PaidBy works. ChangedAt is always
/// stamped with the current UTC time.
/// </summary>
public class CreateItemLogDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class ItemLogDto
{
    public int LogId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime? ChangedAt { get; set; }
}
