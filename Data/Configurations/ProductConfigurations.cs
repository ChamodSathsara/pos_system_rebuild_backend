using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("category");
        builder.HasKey(x => x.CategoryId);
        builder.Property(x => x.CategoryId).HasColumnName("category_id").ValueGeneratedOnAdd();
        builder.Property(x => x.CategoryName).HasColumnName("category_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ParentCategoryId).HasColumnName("parent_category_id");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.ChildCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("brand");
        builder.HasKey(x => x.BrandId);
        builder.Property(x => x.BrandId).HasColumnName("brand_id").ValueGeneratedOnAdd();
        builder.Property(x => x.BrandName).HasColumnName("brand_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
    }
}

public class TaxMasterConfiguration : IEntityTypeConfiguration<TaxMaster>
{
    public void Configure(EntityTypeBuilder<TaxMaster> builder)
    {
        builder.ToTable("tax_master");
        builder.HasKey(x => x.TaxCode);
        builder.Property(x => x.TaxCode).HasColumnName("tax_code").HasMaxLength(20);
        builder.Property(x => x.TaxName).HasColumnName("tax_name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Percentage).HasColumnName("percentage").HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
    }
}

public class ProductMasterConfiguration : IEntityTypeConfiguration<ProductMaster>
{
    public void Configure(EntityTypeBuilder<ProductMaster> builder)
    {
        builder.ToTable("product_master");
        builder.HasKey(x => x.ItemCode);
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.ItemName).HasColumnName("item_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(x => x.CategoryId).HasColumnName("category_id");
        builder.Property(x => x.BrandId).HasColumnName("brand_id");
        builder.Property(x => x.UnitOfMeasure).HasColumnName("unit_of_measure").HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.ItemGroup).HasColumnName("item_group").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Barcode).HasColumnName("barcode").HasMaxLength(100);
        builder.Property(x => x.CostPrice).HasColumnName("cost_price").HasColumnType("decimal(18,2)");
        builder.Property(x => x.SellingPrice).HasColumnName("selling_price").HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReorderLevel).HasColumnName("reorder_level").HasColumnType("decimal(18,3)");
        builder.Property(x => x.TaxCode).HasColumnName("tax_code").HasMaxLength(20);
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.Barcode);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Tax)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.TaxCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
