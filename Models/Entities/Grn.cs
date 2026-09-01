using PosApi.Models.Enums;

namespace PosApi.Models.Entities;

public class GrnMaster
{
    public int GrnId { get; set; }
    public string? GrnNo { get; set; }
    public string? PoNo { get; set; }
    public int? VendorId { get; set; }
    public string? BranchCode { get; set; }
    public string? WarehouseCode { get; set; }
    public DateTime? GrnDate { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime? CreatedAt { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }
    public Vendor? Vendor { get; set; }
    public Branch? Branch { get; set; }
    public Warehouse? Warehouse { get; set; }
    public SystemUser? ReceivedByUser { get; set; }
    public ICollection<GrnItem> Items { get; set; } = new List<GrnItem>();
    public ICollection<GrnReturn> Returns { get; set; } = new List<GrnReturn>();
}

public class GrnItem
{
    public int GrnItemId { get; set; }
    public int? GrnId { get; set; }
    public string? ItemCode { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string? BatchNo { get; set; }
    public DateOnly? ExpiryDate { get; set; }

    public GrnMaster? GrnMaster { get; set; }
    public ProductMaster? Product { get; set; }
    public ICollection<GrnReturnItem> GrnReturnItems { get; set; } = new List<GrnReturnItem>();
}

public class GrnReturn
{
    public int GrnReturnId { get; set; }
    public int? GrnId { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? ReturnBy { get; set; }
    public decimal? TotalReturnAmount { get; set; }
    public string? Reason { get; set; }
    public GrnReturnStatus Status { get; set; }

    public GrnMaster? GrnMaster { get; set; }
    public SystemUser? ReturnByUser { get; set; }
    public ICollection<GrnReturnItem> Items { get; set; } = new List<GrnReturnItem>();
}

public class GrnReturnItem
{
    public int Id { get; set; }
    public int? GrnReturnId { get; set; }
    public int? GrnItemId { get; set; }
    public string? ItemCode { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalAmount { get; set; }

    public GrnReturn? GrnReturn { get; set; }
    public GrnItem? GrnItem { get; set; }
    public ProductMaster? Product { get; set; }
}
