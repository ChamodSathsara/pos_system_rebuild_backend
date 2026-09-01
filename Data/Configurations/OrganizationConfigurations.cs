using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("company");
        builder.HasKey(x => x.CompanyCode);
        builder.Property(x => x.CompanyCode).HasColumnName("company_code").HasMaxLength(50);
        builder.Property(x => x.CompanyName).HasColumnName("company_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(255);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
        builder.Property(x => x.RegistrationNo).HasColumnName("registration_no").HasMaxLength(50);
        builder.Property(x => x.TaxId).HasColumnName("tax_id").HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branch");
        builder.HasKey(x => x.BranchCode);
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50);
        builder.Property(x => x.BranchName).HasColumnName("branch_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(255);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CompanyCode).HasColumnName("company_code").HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Branches)
            .HasForeignKey(x => x.CompanyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouse");
        builder.HasKey(x => x.WarehouseCode);
        builder.Property(x => x.WarehouseCode).HasColumnName("warehouse_code").HasMaxLength(50);
        builder.Property(x => x.WarehouseName).HasColumnName("warehouse_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(255);
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50);
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Warehouses)
            .HasForeignKey(x => x.BranchCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
