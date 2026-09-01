using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("vendor");
        builder.HasKey(x => x.VendorId);
        builder.Property(x => x.VendorId).HasColumnName("vendor_id").ValueGeneratedOnAdd();
        builder.Property(x => x.VendorCode).HasColumnName("vendor_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.VendorName).HasColumnName("vendor_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(255);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
        builder.Property(x => x.ContactPerson).HasColumnName("contact_person").HasMaxLength(100);
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => x.VendorCode).IsUnique();
    }
}

public class VendorLedgerConfiguration : IEntityTypeConfiguration<VendorLedger>
{
    public void Configure(EntityTypeBuilder<VendorLedger> builder)
    {
        builder.ToTable("vendor_ledger");
        builder.HasKey(x => x.LedgerId);
        builder.Property(x => x.LedgerId).HasColumnName("ledger_id").ValueGeneratedOnAdd();
        builder.Property(x => x.VendorId).HasColumnName("vendor_id");
        builder.Property(x => x.GrnTotal).HasColumnName("grn_total").HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReturnTotal).HasColumnName("return_total").HasColumnType("decimal(18,2)");
        builder.Property(x => x.PaidCredit).HasColumnName("paid_credit").HasColumnType("decimal(18,2)");
        builder.Property(x => x.OutstandingBalance).HasColumnName("outstanding_balance").HasColumnType("decimal(18,0)").IsRequired();

        builder.HasOne(x => x.Vendor)
            .WithOne(x => x.VendorLedger)
            .HasForeignKey<VendorLedger>(x => x.VendorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
