using PosApi.Models.Enums;

namespace PosApi.DTOs.Stock;

public class CreateOpeningStockDto
{
    public string ItemCode { get; set; } = string.Empty;
    /// <summary>Required for branch warehouses; null for a central warehouse.</summary>
    public string? BranchCode { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }

    public DateOnly? ExpiryDate { get; set; }
    public DateTime? OpeningDate { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
}

public class OpeningStockDto
{
    public int StockId { get; set; }

    public long BatchId { get; set; }

    public string BatchNo { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal TotalValue { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateTime OpeningDate { get; set; }

    public string? ReferenceNo { get; set; }

    public StockReferenceType ReferenceType { get; set; }
}
