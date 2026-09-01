using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class GrnMasterConfiguration : IEntityTypeConfiguration<GrnMaster>
{
    public void Configure(EntityTypeBuilder<GrnMaster> builder)
    {
        builder.ToTable("grn_master");
        builder.HasKey(x => x.GrnId);
        builder.Property(x => x.GrnId).HasColumnName("grn_id").ValueGeneratedOnAdd();
        builder.Property(x => x.GrnNo).HasColumnName("grn_no").HasMaxLength(50);
        builder.Property(x => x.PoNo).HasColumnName("po_no").HasMaxLength(50);
        builder.Property(x => x.VendorId).HasColumnName("vendor_id");
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50);
        builder.Property(x => x.WarehouseCode).HasColumnName("warehouse_code").HasMaxLength(50);
        builder.Property(x => x.GrnDate).HasColumnName("grn_date");
        builder.Property(x => x.InvoiceNo).HasColumnName("invoice_no").HasMaxLength(50);
        builder.Property(x => x.InvoiceDate).HasColumnName("invoice_date");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(255);
        builder.Property(x => x.ReceivedBy).HasColumnName("received_by").HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.PurchaseOrder).WithMany(x => x.GrnMasters).HasForeignKey(x => x.PoNo).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Vendor).WithMany(x => x.GrnMasters).HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReceivedByUser).WithMany().HasForeignKey(x => x.ReceivedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GrnItemConfiguration : IEntityTypeConfiguration<GrnItem>
{
    public void Configure(EntityTypeBuilder<GrnItem> builder)
    {
        builder.ToTable("grn_item");
        builder.HasKey(x => x.GrnItemId);
        builder.Property(x => x.GrnItemId).HasColumnName("grn_item_id").ValueGeneratedOnAdd();
        builder.Property(x => x.GrnId).HasColumnName("grn_id");
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalCost).HasColumnName("total_cost").HasColumnType("decimal(18,2)");
        builder.Property(x => x.BatchNo).HasColumnName("batch_no").HasMaxLength(50);
        builder.Property(x => x.ExpiryDate).HasColumnName("expiry_date");

        builder.HasOne(x => x.GrnMaster).WithMany(x => x.Items).HasForeignKey(x => x.GrnId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GrnReturnConfiguration : IEntityTypeConfiguration<GrnReturn>
{
    public void Configure(EntityTypeBuilder<GrnReturn> builder)
    {
        builder.ToTable("grn_return");
        builder.HasKey(x => x.GrnReturnId);
        builder.Property(x => x.GrnReturnId).HasColumnName("grn_return_id").ValueGeneratedOnAdd();
        builder.Property(x => x.GrnId).HasColumnName("grn_id");
        builder.Property(x => x.ReturnDate).HasColumnName("return_date");
        builder.Property(x => x.ReturnBy).HasColumnName("return_by").HasMaxLength(50);
        builder.Property(x => x.TotalReturnAmount).HasColumnName("total_return_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(255);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.GrnMaster).WithMany(x => x.Returns).HasForeignKey(x => x.GrnId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReturnByUser).WithMany().HasForeignKey(x => x.ReturnBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GrnReturnItemConfiguration : IEntityTypeConfiguration<GrnReturnItem>
{
    public void Configure(EntityTypeBuilder<GrnReturnItem> builder)
    {
        builder.ToTable("grn_return_item");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.GrnReturnId).HasColumnName("grn_return_id");
        builder.Property(x => x.GrnItemId).HasColumnName("grn_item_id");
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.GrnReturn).WithMany(x => x.Items).HasForeignKey(x => x.GrnReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.GrnItem).WithMany(x => x.GrnReturnItems).HasForeignKey(x => x.GrnItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
    }
}
