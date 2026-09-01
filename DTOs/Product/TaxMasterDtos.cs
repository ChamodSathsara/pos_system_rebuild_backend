namespace PosApi.DTOs.Product;

public class CreateTaxMasterDto
{
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTaxMasterDto
{
    public string TaxName { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class TaxMasterDto
{
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
