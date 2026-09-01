using PosApi.Models.Enums;

namespace PosApi.DTOs.Purchase;

public class CreatePurchaseOrderHistoryChangeDto
{
    public int HistoryId { get; set; }
    public PurchaseOrderChangeField Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class UpdatePurchaseOrderHistoryChangeDto
{
    public PurchaseOrderChangeField Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class PurchaseOrderHistoryChangeDto
{
    public int Id { get; set; }
    public int? HistoryId { get; set; }
    public PurchaseOrderChangeField Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}