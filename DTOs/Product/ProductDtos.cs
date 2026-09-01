using PosApi.Models.Enums;

namespace PosApi.DTOs.Product;

public class CreateProductDto
{
    /// <summary>Optional. Auto-generated (e.g. ITM00001) when omitted.</summary>
    public string? ItemCode { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public ItemGroup ItemGroup { get; set; }
    public string? Barcode { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? ReorderLevel { get; set; }
    public string? TaxCode { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProductDto
{
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public ItemGroup ItemGroup { get; set; }
    public string? Barcode { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? ReorderLevel { get; set; }
    public string? TaxCode { get; set; }
    public bool IsActive { get; set; }
}

public class ProductDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public ItemGroup ItemGroup { get; set; }
    public string? Barcode { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? ReorderLevel { get; set; }
    public string? TaxCode { get; set; }
    public decimal? TaxPercentage { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
