using PosApi.Models.Enums;

namespace PosApi.DTOs.Stock;

public class CreateStockTransferRequestDto
{
    public string SourceWarehouseCode { get; set; } = string.Empty;
    public string DestinationWarehouseCode { get; set; } = string.Empty;
    public DateTime? RequiredDate { get; set; }
    public string? Remarks { get; set; }
    public List<CreateStockTransferRequestLineDto> Lines { get; set; } = [];
}
public class CreateStockTransferRequestLineDto { public string ItemCode { get; set; } = string.Empty; public decimal Quantity { get; set; } public string? Remarks { get; set; } }
public class AcceptStockTransferRequestDto { public List<ApproveStockTransferLineDto> Lines { get; set; } = []; public string? Remarks { get; set; } }
public class ApproveStockTransferLineDto { public long TransferRequestLineId { get; set; } public decimal ApprovedQty { get; set; } }
public class CreateStockTransferDispatchDto { public string? VehicleNo { get; set; } public string? DriverName { get; set; } public string? Remarks { get; set; } public List<CreateStockTransferDispatchLineDto> Lines { get; set; } = []; }
public class CreateStockTransferDispatchLineDto { public long TransferRequestLineId { get; set; } public long BatchId { get; set; } public decimal Quantity { get; set; } }
/// <summary>InventoryClerk dispatches central stock directly to a branch without a branch request/PO.</summary>
public class CreateDirectStockTransferDto { public string DestinationWarehouseCode { get; set; } = string.Empty; public string? VehicleNo { get; set; } public string? DriverName { get; set; } public string? Remarks { get; set; } public List<CreateDirectStockTransferLineDto> Lines { get; set; } = []; }
public class CreateDirectStockTransferLineDto { public string ItemCode { get; set; } = string.Empty; public long BatchId { get; set; } public decimal Quantity { get; set; } public string? Remarks { get; set; } }
public class ReceiveStockTransferDispatchDto { public string? Remarks { get; set; } public List<ReceiveStockTransferLineDto> Lines { get; set; } = []; }
public class ReceiveStockTransferLineDto { public long DispatchLineId { get; set; } public decimal ReceivedQty { get; set; } public decimal DamagedQty { get; set; } public decimal ShortQty { get; set; } public string? Remarks { get; set; } }

public class StockTransferRequestDto
{
    public long TransferRequestId { get; set; } public string RequestNo { get; set; } = string.Empty; public string SourceWarehouseCode { get; set; } = string.Empty; public string DestinationWarehouseCode { get; set; } = string.Empty; public StockTransferStatus Status { get; set; } public DateTime RequestDate { get; set; } public DateTime? RequiredDate { get; set; } public string? Remarks { get; set; } public List<StockTransferRequestLineDto> Lines { get; set; } = [];
}
public class StockTransferRequestLineDto { public long TransferRequestLineId { get; set; } public string ItemCode { get; set; } = string.Empty; public decimal RequestedQty { get; set; } public decimal ApprovedQty { get; set; } public decimal DispatchedQty { get; set; } public decimal ReceivedQty { get; set; } public string? Remarks { get; set; } }
public class StockTransferDispatchDto { public long DispatchId { get; set; } public string DispatchNo { get; set; } = string.Empty; public long TransferRequestId { get; set; } public DateTime DispatchedAt { get; set; } public string? VehicleNo { get; set; } public string? DriverName { get; set; } public List<StockTransferDispatchLineDto> Lines { get; set; } = []; }
public class StockTransferDispatchLineDto { public long DispatchLineId { get; set; } public long TransferRequestLineId { get; set; } public long BatchId { get; set; } public decimal Quantity { get; set; } public decimal UnitCost { get; set; } }
public class StockTransferReceiptDto { public long ReceiptId { get; set; } public string ReceiptNo { get; set; } = string.Empty; public long DispatchId { get; set; } public DateTime ReceivedAt { get; set; } }
