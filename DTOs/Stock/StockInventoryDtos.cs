namespace PosApi.DTOs.Stock;

public class CreateStockInventoryDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
}

public class StockInventoryDto
{
    public int StockId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public decimal CurrentQty { get; set; }
    public DateTime LastUpdated { get; set; }
}
