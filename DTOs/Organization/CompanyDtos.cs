namespace PosApi.DTOs.Organization;

public class CreateCompanyDto
{
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? RegistrationNo { get; set; }
    public string? TaxId { get; set; }
}

public class UpdateCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? RegistrationNo { get; set; }
    public string? TaxId { get; set; }
}

public class CompanyDto
{
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? RegistrationNo { get; set; }
    public string? TaxId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
