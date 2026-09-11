namespace PosApi.Models.Entities;

public class CentralStockReceipt
{
    public long ReceiptId { get; set; }
    public string ReceiptNo { get; set; } = null!;
    public string WarehouseCode { get; set; } = null!;
    public DateTime ReceiptDate { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public string ReceivedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public Warehouse? Warehouse { get; set; }
    public SystemUser? ReceivedByUser { get; set; }
    public ICollection<CentralStockReceiptLine> Lines { get; set; } = new List<CentralStockReceiptLine>();
}

public class CentralStockReceiptLine
{
    public long ReceiptLineId { get; set; }
    public long ReceiptId { get; set; }
    public string ItemCode { get; set; } = null!;
    public long BatchId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public DateOnly? ExpiryDate { get; set; }

    public CentralStockReceipt? Receipt { get; set; }
    public ProductMaster? Product { get; set; }
    public StockBatch? Batch { get; set; }
}
