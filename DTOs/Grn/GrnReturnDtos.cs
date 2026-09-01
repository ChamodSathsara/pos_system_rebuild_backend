using PosApi.Models.Enums;

namespace PosApi.DTOs.Grn;

public class CreateGrnReturnItemLineDto
{
    /// <summary>The GrnItem being returned to the vendor. Must belong to the GRN referenced by the parent request.</summary>
    public int GrnItemId { get; set; }
    public decimal Quantity { get; set; }
}

/// <summary>
/// Returns previously received stock back to a vendor against a specific GRN. Unit cost for each
/// line is taken from the original GrnItem, not supplied by the caller. Posting a return mirrors
/// the GRN receiving flow in reverse, in one transaction: it inserts the return header/lines,
/// reduces StockInventory, draws down the matching StockBatch (closing it out if it's now empty),
/// raises a STOCK_OUT StockMovement, rolls back the source PurchaseOrderItem's ReceivedQuantity,
/// recomputes the PurchaseOrder's status, appends a PurchaseOrderHistory entry, and posts the
/// return total to the vendor's ledger.
/// </summary>
public class CreateGrnReturnDto
{
    public int GrnId { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Reason { get; set; }
    public List<CreateGrnReturnItemLineDto> Items { get; set; } = new();
}

public class GrnReturnItemDto
{
    public int Id { get; set; }
    public int? GrnReturnId { get; set; }
    public int? GrnItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalAmount { get; set; }
}

public class GrnReturnDto
{
    public int GrnReturnId { get; set; }
    public int? GrnId { get; set; }
    public string? GrnNo { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? ReturnBy { get; set; }
    public decimal? TotalReturnAmount { get; set; }
    public string? Reason { get; set; }
    public GrnReturnStatus Status { get; set; }
    public List<GrnReturnItemDto> Items { get; set; } = new();
}
