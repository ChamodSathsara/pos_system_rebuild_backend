namespace PosApi.DTOs.Product;

public class CreateCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
