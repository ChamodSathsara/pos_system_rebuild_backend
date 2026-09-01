using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_order");
        builder.HasKey(x => x.PoNo);
        builder.Property(x => x.PoNo).HasColumnName("po_no").HasMaxLength(50);
        builder.Property(x => x.VendorId).HasColumnName("vendor_id");
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50);
        builder.Property(x => x.PoDate).HasColumnName("po_date");
        builder.Property(x => x.ExpectedDate).HasColumnName("expected_date");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(255);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(x => x.Vendor).WithMany(x => x.PurchaseOrders).HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("purchase_order_item");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.PoNo).HasColumnName("po_no").HasMaxLength(50);
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,3)");
        builder.Property(x => x.ReceivedQuantity).HasColumnName("received_quantity").HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalCost).HasColumnName("total_cost").HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.PurchaseOrder).WithMany(x => x.Items).HasForeignKey(x => x.PoNo).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseOrderHistoryConfiguration : IEntityTypeConfiguration<PurchaseOrderHistory>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderHistory> builder)
    {
        builder.ToTable("purchase_order_history");
        builder.HasKey(x => x.HistoryId);
        builder.Property(x => x.HistoryId).HasColumnName("history_id").ValueGeneratedOnAdd();
        builder.Property(x => x.PoNo).HasColumnName("po_no").HasMaxLength(50);
        builder.Property(x => x.Action).HasColumnName("action").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ChangedBy).HasColumnName("changed_by").HasMaxLength(50);
        builder.Property(x => x.ChangedAt).HasColumnName("changed_at");
        builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(255);

        builder.HasOne(x => x.PurchaseOrder).WithMany(x => x.Histories).HasForeignKey(x => x.PoNo).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseOrderHistoryChangeConfiguration : IEntityTypeConfiguration<PurchaseOrderHistoryChange>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderHistoryChange> builder)
    {
        builder.ToTable("purchase_order_history_change");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.HistoryId).HasColumnName("history_id");
        builder.Property(x => x.Field).HasColumnName("field").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.OldValue).HasColumnName("old_value").HasMaxLength(255);
        builder.Property(x => x.NewValue).HasColumnName("new_value").HasMaxLength(255);

        builder.HasOne(x => x.History).WithMany(x => x.Changes).HasForeignKey(x => x.HistoryId).OnDelete(DeleteBehavior.Cascade);
    }
}
