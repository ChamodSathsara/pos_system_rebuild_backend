namespace PosApi.DTOs.Organization;

public class CreateWarehouseDto
{
    public string? WarehouseCode { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? BranchCode { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCentralWarehouse { get; set; }
    public string? ParentWarehouseCode { get; set; }
}

public class UpdateWarehouseDto
{
    public string WarehouseName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? BranchCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsCentralWarehouse { get; set; }
    public string? ParentWarehouseCode { get; set; }
}

public class WarehouseDto
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? BranchCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsCentralWarehouse { get; set; }
    public string? ParentWarehouseCode { get; set; }
}
