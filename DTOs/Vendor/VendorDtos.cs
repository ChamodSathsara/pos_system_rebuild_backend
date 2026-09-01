namespace PosApi.DTOs.Vendor;

public class CreateVendorDto
{
    /// <summary>Optional. Auto-generated (e.g. VEN00001) when omitted.</summary>
    public string? VendorCode { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateVendorDto
{
    public string VendorName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsActive { get; set; }
}

public class VendorDto
{
    public int VendorId { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Outstanding balance snapshot, present whenever the vendor's ledger has been loaded.</summary>
    public decimal? OutstandingBalance { get; set; }
}
