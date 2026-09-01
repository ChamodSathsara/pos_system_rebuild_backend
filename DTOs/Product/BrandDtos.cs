namespace PosApi.DTOs.Product;

public class CreateBrandDto
{
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateBrandDto
{
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class BrandDto
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
