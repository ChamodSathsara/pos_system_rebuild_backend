namespace PosApi.DTOs.Vendor;

public class VendorLedgerDto
{
    public int LedgerId { get; set; }
    public int? VendorId { get; set; }
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }
    public decimal? GrnTotal { get; set; }
    public decimal? ReturnTotal { get; set; }
    public decimal? PaidCredit { get; set; }
    public decimal OutstandingBalance { get; set; }
}

/// <summary>
/// Creates a ledger for a vendor that doesn't already have one (self-heal / migration scenario).
/// Normal vendor creation already provisions a zeroed ledger automatically.
/// </summary>
public class CreateVendorLedgerDto
{
    public int VendorId { get; set; }
    public decimal GrnTotal { get; set; }
    public decimal ReturnTotal { get; set; }
    public decimal PaidCredit { get; set; }
}

/// <summary>
/// Administrative correction of ledger totals. Prefer <see cref="RecordVendorPaymentDto"/> for
/// everyday "vendor paid us back" flows - this endpoint is for fixing totals directly.
/// </summary>
public class UpdateVendorLedgerDto
{
    public decimal GrnTotal { get; set; }
    public decimal ReturnTotal { get; set; }
    public decimal PaidCredit { get; set; }
}

public class RecordVendorPaymentDto
{
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }
}
