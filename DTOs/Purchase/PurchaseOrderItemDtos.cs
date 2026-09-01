namespace PosApi.DTOs.Purchase;

/// <summary>Adds a single line item to an existing open purchase order (as opposed to CreatePurchaseOrderDto, which creates the order and its items together).</summary>
public class CreatePurchaseOrderItemDto
{
    public string PoNo { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class UpdatePurchaseOrderItemDto
{
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}