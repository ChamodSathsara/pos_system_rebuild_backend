using PosApi.Models.Enums;

namespace PosApi.Models.Entities;

public class Company
{
    public string CompanyCode { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? RegistrationNo { get; set; }
    public string? TaxId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}

public class Branch
{
    public string BranchCode { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public BranchStatus Status { get; set; }
    public string? CompanyCode { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Company? Company { get; set; }
    public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public ICollection<SystemUser> SystemUsers { get; set; } = new List<SystemUser>();
}

public class Warehouse
{
    public string WarehouseCode { get; set; } = null!;
    public string WarehouseName { get; set; } = null!;
    public string? Address { get; set; }
    public string? BranchCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }

    public Branch? Branch { get; set; }
}
