using PosApi.Models.Enums;

namespace PosApi.DTOs.Stock;

/// <summary>
/// Records a manual stock movement (adjustment, correction, transfer leg, etc) against a batch.
/// Qty is signed: positive increases the batch's AvailableQty, negative decreases it. The parent
/// StockInventory's CurrentQty is kept in sync automatically.
/// </summary>
public class CreateStockMovementDto
{
    public int StockId { get; set; }
    public long BatchId { get; set; }
    public StockMovementType MovementType { get; set; } = StockMovementType.Adjustment;
    public StockReferenceType ReferenceType { get; set; } = StockReferenceType.StockAdjustment;
    public string? ReferenceNo { get; set; }
    public decimal Qty { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// Movements are an immutable audit trail: only the descriptive fields can be corrected here.
/// Quantities can never be edited - record a new movement (or delete the most recent one) instead.
/// </summary>
public class UpdateStockMovementDto
{
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
}

public class StockMovementDto
{
    public long MovementId { get; set; }
    public long BatchId { get; set; }
    public int StockId { get; set; }
    public StockMovementType MovementType { get; set; }
    public StockReferenceType ReferenceType { get; set; }
    public string? ReferenceNo { get; set; }
    public decimal Qty { get; set; }
    public decimal PreviousQty { get; set; }
    public decimal NewQty { get; set; }
    public decimal UnitCost { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
