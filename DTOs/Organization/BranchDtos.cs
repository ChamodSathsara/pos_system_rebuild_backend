using PosApi.Models.Enums;

namespace PosApi.DTOs.Organization;

public class CreateBranchDto
{
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public BranchStatus Status { get; set; } = BranchStatus.Active;
    public string? CompanyCode { get; set; }
}

public class UpdateBranchDto
{
    public string BranchName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public BranchStatus Status { get; set; }
    public string? CompanyCode { get; set; }
}

public class BranchDto
{
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public BranchStatus Status { get; set; }
    public string? CompanyCode { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
