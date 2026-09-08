using PosApi.Models.Enums;

namespace PosApi.Models.Entities;

/// <summary>Internal purchase-order-like request from a branch warehouse to a central warehouse.</summary>
public class StockTransferRequest
{
    public long TransferRequestId { get; set; }
    public string RequestNo { get; set; } = null!;
    public string SourceWarehouseCode { get; set; } = null!;
    public string DestinationWarehouseCode { get; set; } = null!;
    public DateTime RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public StockTransferStatus Status { get; set; }
    public string? Remarks { get; set; }
    public string RequestedBy { get; set; } = null!;
    public string? AcceptedBy { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Warehouse? SourceWarehouse { get; set; }
    public Warehouse? DestinationWarehouse { get; set; }
    public ICollection<StockTransferRequestLine> Lines { get; set; } = new List<StockTransferRequestLine>();
    public ICollection<StockTransferDispatch> Dispatches { get; set; } = new List<StockTransferDispatch>();
}

public class StockTransferRequestLine
{
    public long TransferRequestLineId { get; set; }
    public long TransferRequestId { get; set; }
    public string ItemCode { get; set; } = null!;
    public decimal RequestedQty { get; set; }
    public decimal ApprovedQty { get; set; }
    public decimal DispatchedQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public string? Remarks { get; set; }
    public StockTransferRequest? TransferRequest { get; set; }
    public ProductMaster? Product { get; set; }
}

public class StockTransferDispatch
{
    public long DispatchId { get; set; }
    public string DispatchNo { get; set; } = null!;
    public long TransferRequestId { get; set; }
    public string? VehicleNo { get; set; }
    public string? DriverName { get; set; }
    public string? Remarks { get; set; }
    public string DispatchedBy { get; set; } = null!;
    public DateTime DispatchedAt { get; set; }
    public StockTransferRequest? TransferRequest { get; set; }
    public ICollection<StockTransferDispatchLine> Lines { get; set; } = new List<StockTransferDispatchLine>();
    public StockTransferReceipt? Receipt { get; set; }
}

public class StockTransferDispatchLine
{
    public long DispatchLineId { get; set; }
    public long DispatchId { get; set; }
    public long TransferRequestLineId { get; set; }
    public long BatchId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public StockTransferDispatch? Dispatch { get; set; }
    public StockTransferRequestLine? TransferRequestLine { get; set; }
    public StockBatch? StockBatch { get; set; }
}

public class StockTransferReceipt
{
    public long ReceiptId { get; set; }
    public string ReceiptNo { get; set; } = null!;
    public long DispatchId { get; set; }
    public string ReceivedBy { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
    public string? Remarks { get; set; }
    public StockTransferDispatch? Dispatch { get; set; }
    public ICollection<StockTransferReceiptLine> Lines { get; set; } = new List<StockTransferReceiptLine>();
}

public class StockTransferReceiptLine
{
    public long ReceiptLineId { get; set; }
    public long ReceiptId { get; set; }
    public long DispatchLineId { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal ShortQty { get; set; }
    public decimal DamagedQty { get; set; }
    public string? Remarks { get; set; }
    public StockTransferReceipt? Receipt { get; set; }
    public StockTransferDispatchLine? DispatchLine { get; set; }
}
