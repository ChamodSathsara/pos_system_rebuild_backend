using PosApi.Models.Enums;

namespace PosApi.DTOs.Purchase;

public class CreatePurchaseOrderItemLineDto
{
    public string ItemCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class CreatePurchaseOrderDto
{
    /// <summary>Optional. Auto-generated (e.g. PO000001) when omitted.</summary>
    public string? PoNo { get; set; }
    public int VendorId { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public DateTime? PoDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public string? Remarks { get; set; }
    public List<CreatePurchaseOrderItemLineDto> Items { get; set; } = new();
}

/// <summary>
/// Full update of an open purchase order's header and line items. Only permitted while the
/// order is Open and nothing has been received against it yet - use the dedicated
/// PurchaseOrderItems endpoints for surgical single-line edits instead if preferred.
/// </summary>
public class UpdatePurchaseOrderDto
{
    public DateTime? ExpectedDate { get; set; }
    public string? Remarks { get; set; }
    public List<CreatePurchaseOrderItemLineDto> Items { get; set; } = new();
}

public class CancelPurchaseOrderDto
{
    public string? Remarks { get; set; }
}

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public string? PoNo { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
}

public class PurchaseOrderDto
{
    public string PoNo { get; set; } = string.Empty;
    public int? VendorId { get; set; }
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }
    public string? BranchCode { get; set; }
    public DateTime? PoDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}