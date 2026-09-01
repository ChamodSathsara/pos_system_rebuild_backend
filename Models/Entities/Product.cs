using PosApi.Models.Enums;

namespace PosApi.Models.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public Category? ParentCategory { get; set; }
    public ICollection<Category> ChildCategories { get; set; } = new List<Category>();
    public ICollection<ProductMaster> Products { get; set; } = new List<ProductMaster>();
}

public class Brand
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<ProductMaster> Products { get; set; } = new List<ProductMaster>();
}

public class TaxMaster
{
    public string TaxCode { get; set; } = null!;
    public string TaxName { get; set; } = null!;
    public decimal Percentage { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<ProductMaster> Products { get; set; } = new List<ProductMaster>();
}

public class ProductMaster
{
    public string ItemCode { get; set; } = null!;
    public string ItemName { get; set; } = null!;
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
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Category? Category { get; set; }
    public Brand? Brand { get; set; }
    public TaxMaster? Tax { get; set; }
}
