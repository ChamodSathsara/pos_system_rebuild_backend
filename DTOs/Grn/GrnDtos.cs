namespace PosApi.DTOs.Grn;

public class CreateGrnItemLineDto
{
    public string ItemCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }

    /// <summary>Optional. Auto-generated from the GRN number and item code when omitted.</summary>
    public string? BatchNo { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}

/// <summary>
/// Creates a GRN against an open/partially-received purchase order. Every line item must match an
/// item already on the PO and the received quantity can never exceed what's still outstanding on
/// that line. Posting a GRN drives the full receiving flow in one transaction: it inserts the GRN
/// header/lines, tops up StockInventory, opens a new StockBatch per line, raises a STOCK_IN
/// StockMovement, updates the source PurchaseOrderItem's ReceivedQuantity, recomputes the
/// PurchaseOrder's status, appends a PurchaseOrderHistory entry, and posts the GRN total to the
/// vendor's ledger.
/// </summary>
public class CreateGrnDto
{
    /// <summary>Optional. Auto-generated (e.g. GRN000001) when omitted.</summary>
    public string? GrnNo { get; set; }
    public string PoNo { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public DateTime? GrnDate { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? Remarks { get; set; }
    public List<CreateGrnItemLineDto> Items { get; set; } = new();
}

public class GrnItemDto
{
    public int GrnItemId { get; set; }
    public int? GrnId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string? BatchNo { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}

public class GrnDto
{
    public int GrnId { get; set; }
    public string? GrnNo { get; set; }
    public string? PoNo { get; set; }
    public int? VendorId { get; set; }
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }
    public string? BranchCode { get; set; }
    public string? WarehouseCode { get; set; }
    public DateTime? GrnDate { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<GrnItemDto> Items { get; set; } = new();
}
