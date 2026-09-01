using PosApi.Models.Enums;

namespace PosApi.DTOs.Pos;

public class PosTerminalItemDto
{
    public int StockId { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Barcode { get; set; }

    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public int? BrandId { get; set; }

    public string? BrandName { get; set; }

    public UnitOfMeasure UnitOfMeasure { get; set; }

    public ItemGroup ItemGroup { get; set; }

    public decimal Price { get; set; }

    public decimal AvailableQty { get; set; }

    public decimal? ReorderLevel { get; set; }

    public string? TaxCode { get; set; }

    public decimal TaxPercentage { get; set; }

    public string BranchCode { get; set; } = string.Empty;

    public string WarehouseCode { get; set; } = string.Empty;

    public bool IsAvailable => AvailableQty > 0;
}