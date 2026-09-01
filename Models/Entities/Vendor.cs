namespace PosApi.Models.Entities;

public class Vendor
{
    public int VendorId { get; set; }
    public string VendorCode { get; set; } = null!;
    public string VendorName { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public VendorLedger? VendorLedger { get; set; }
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public ICollection<GrnMaster> GrnMasters { get; set; } = new List<GrnMaster>();
}

public class VendorLedger
{
    public int LedgerId { get; set; }
    public int? VendorId { get; set; }
    public decimal? GrnTotal { get; set; }
    public decimal? ReturnTotal { get; set; }
    public decimal? PaidCredit { get; set; }
    public decimal OutstandingBalance { get; set; }

    public Vendor? Vendor { get; set; }
}
