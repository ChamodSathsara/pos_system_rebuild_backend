using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class CentralStockReceiptConfiguration : IEntityTypeConfiguration<CentralStockReceipt>
{
    public void Configure(EntityTypeBuilder<CentralStockReceipt> builder)
    {
        builder.ToTable("central_stock_receipt");
        builder.HasKey(x => x.ReceiptId);
        builder.Property(x => x.ReceiptId).HasColumnName("receipt_id").ValueGeneratedOnAdd();
        builder.Property(x => x.ReceiptNo).HasColumnName("receipt_no").HasMaxLength(50).IsRequired();
        builder.Property(x => x.WarehouseCode).HasColumnName("warehouse_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReceiptDate).HasColumnName("receipt_date").IsRequired();
        builder.Property(x => x.ReferenceNo).HasColumnName("reference_no").HasMaxLength(100);
        builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(500);
        builder.Property(x => x.TotalQuantity).HasColumnName("total_quantity").HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.TotalCost).HasColumnName("total_cost").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ReceivedBy).HasColumnName("received_by").HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(x => x.ReceiptNo).IsUnique();
        builder.HasIndex(x => new { x.WarehouseCode, x.ReceiptDate });
        builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReceivedByUser).WithMany().HasForeignKey(x => x.ReceivedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CentralStockReceiptLineConfiguration : IEntityTypeConfiguration<CentralStockReceiptLine>
{
    public void Configure(EntityTypeBuilder<CentralStockReceiptLine> builder)
    {
        builder.ToTable("central_stock_receipt_line");
        builder.HasKey(x => x.ReceiptLineId);
        builder.Property(x => x.ReceiptLineId).HasColumnName("receipt_line_id").ValueGeneratedOnAdd();
        builder.Property(x => x.ReceiptId).HasColumnName("receipt_id").IsRequired();
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.BatchId).HasColumnName("batch_id").IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.SellingPrice).HasColumnName("selling_price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
        builder.HasOne(x => x.Receipt).WithMany(x => x.Lines).HasForeignKey(x => x.ReceiptId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
    }
}
