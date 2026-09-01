using PosApi.Models.Enums;

namespace PosApi.DTOs.Stock;

/// <summary>Receives new stock into a batch. This also raises an "In" stock movement and increases the parent StockInventory's CurrentQty.</summary>
public class CreateStockBatchDto
{
    public int StockId { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public decimal ReceivedQty { get; set; }
    public decimal UnitCost { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? ReferenceNo { get; set; }
    public StockReferenceType ReferenceType { get; set; } = StockReferenceType.StockReceive;
    public string? Remarks { get; set; }
}

/// <summary>
/// Metadata correction for an existing batch. Quantities are never edited here - moving a batch's
/// status to Damaged/Expired/Blocked automatically writes off any remaining AvailableQty via a
/// generated adjustment movement so stock levels stay consistent.
/// </summary>
public class UpdateStockBatchDto
{
    public string BatchNo { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public BatchStatus Status { get; set; }
    public string? Remarks { get; set; }
}

public class StockBatchDto
{
    public long BatchId { get; set; }
    public int StockId { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public decimal ReceivedQty { get; set; }
    public decimal AvailableQty { get; set; }
    public decimal UnitCost { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public DateTime ReceivedDate { get; set; }
    public BatchStatus Status { get; set; }
}
