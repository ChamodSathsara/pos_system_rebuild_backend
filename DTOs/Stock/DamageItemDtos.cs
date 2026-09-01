using PosApi.Models.Enums;

namespace PosApi.DTOs.Stock;

/// <summary>
/// Records a newly reported damaged/written-off item. ReportedBy is not accepted here - it is
/// always set to the currently authenticated user recording the report, the same way
/// Expense.PaidBy works. Status always starts at Reported.
/// </summary>
public class CreateDamageItemDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string? WarehouseCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal? CostAmount { get; set; }
    public string? Reason { get; set; }
    public DateTime? DamageDate { get; set; }
}

/// <summary>
/// Updates a damage report, including moving it through its review workflow via Status
/// (Reported -> Reviewed -> Approved -> Disposed, or Rejected). ReportedBy is an immutable
/// audit field set at creation time and cannot be changed here.
/// </summary>
public class UpdateDamageItemDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string? WarehouseCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal? CostAmount { get; set; }
    public string? Reason { get; set; }
    public DateTime? DamageDate { get; set; }
    public DamageItemStatus Status { get; set; }
}

public class DamageItemDto
{
    public int DamageId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? CostAmount { get; set; }
    public string? Reason { get; set; }
    public DateTime? DamageDate { get; set; }
    public string? ReportedBy { get; set; }
    public string? ReportedByName { get; set; }
    public DamageItemStatus Status { get; set; }
}
