using PosApi.Models.Enums;

namespace PosApi.Models.Entities;

public class PurchaseOrder
{
    public string PoNo { get; set; } = null!;
    public int? VendorId { get; set; }
    public string? BranchCode { get; set; }
    public DateTime? PoDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Vendor? Vendor { get; set; }
    public Branch? Branch { get; set; }
    public SystemUser? CreatedByUser { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<PurchaseOrderHistory> Histories { get; set; } = new List<PurchaseOrderHistory>();
    public ICollection<GrnMaster> GrnMasters { get; set; } = new List<GrnMaster>();
}

public class PurchaseOrderItem
{
    public int Id { get; set; }
    public string? PoNo { get; set; }
    public string? ItemCode { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }
    public ProductMaster? Product { get; set; }
}

public class PurchaseOrderHistory
{
    public int HistoryId { get; set; }
    public string? PoNo { get; set; }
    public PurchaseOrderHistoryAction Action { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime? ChangedAt { get; set; }
    public string? Remarks { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }
    public SystemUser? ChangedByUser { get; set; }
    public ICollection<PurchaseOrderHistoryChange> Changes { get; set; } = new List<PurchaseOrderHistoryChange>();
}

public class PurchaseOrderHistoryChange
{
    public int Id { get; set; }
    public int? HistoryId { get; set; }
    public PurchaseOrderChangeField Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public PurchaseOrderHistory? History { get; set; }
}
