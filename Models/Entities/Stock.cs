using PosApi.Models.Enums;

namespace PosApi.Models.Entities;

public class StockInventory
{
    public int StockId { get; set; }
    public string ItemCode { get; set; } = null!;
    public string BranchCode { get; set; } = null!;
    public string WarehouseCode { get; set; } = null!;
    public decimal CurrentQty { get; set; }
    public DateTime LastUpdated { get; set; }

    public ProductMaster? Product { get; set; }
    public Branch? Branch { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<StockBatch> Batches { get; set; } = new List<StockBatch>();
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}

public class StockBatch
{
    public long BatchId { get; set; }
    public int StockId { get; set; }
    public string BatchNo { get; set; } = null!;
    public decimal ReceivedQty { get; set; }
    public decimal AvailableQty { get; set; }
    public decimal UnitCost { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public DateTime ReceivedDate { get; set; }
    public BatchStatus Status { get; set; }

    public StockInventory? StockInventory { get; set; }
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}

public class StockMovement
{
    public long MovementId { get; set; }
    public long BatchId { get; set; }
    public int StockId { get; set; }
    public StockMovementType MovementType { get; set; }
    public string? ReferenceNo { get; set; }
    public decimal Qty { get; set; }
    public decimal PreviousQty { get; set; }
    public decimal NewQty { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public decimal UnitCost { get; set; }
    public StockReferenceType ReferenceType { get; set; }

    public StockBatch? StockBatch { get; set; }
    public StockInventory? StockInventory { get; set; }
    public SystemUser? CreatedByUser { get; set; }
}
