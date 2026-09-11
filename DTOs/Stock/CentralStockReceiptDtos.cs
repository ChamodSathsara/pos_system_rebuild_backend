namespace PosApi.DTOs.Stock;

public class CreateCentralStockReceiptDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public DateTime? ReceiptDate { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
    public List<CreateCentralStockReceiptLineDto> Items { get; set; } = [];
}

public class CreateCentralStockReceiptLineDto
{
    public string ItemCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}

public class CentralStockReceiptDto
{
    public long ReceiptId { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string? WarehouseName { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<CentralStockReceiptLineDto> Items { get; set; } = [];
}

public class CentralStockReceiptLineDto
{
    public long ReceiptLineId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public long BatchId { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TotalCost { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}
