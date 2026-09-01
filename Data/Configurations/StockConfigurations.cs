using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class StockInventoryConfiguration : IEntityTypeConfiguration<StockInventory>
{
    public void Configure(EntityTypeBuilder<StockInventory> builder)
    {
        builder.ToTable("stock_inventory");
        builder.HasKey(x => x.StockId);
        builder.Property(x => x.StockId).HasColumnName("stock_id").ValueGeneratedOnAdd();
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.WarehouseCode).HasColumnName("warehouse_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.CurrentQty).HasColumnName("current_qty").HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.LastUpdated).HasColumnName("last_updated").IsRequired();

        builder.HasIndex(x => new { x.ItemCode, x.BranchCode, x.WarehouseCode }).IsUnique();

        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockBatchConfiguration : IEntityTypeConfiguration<StockBatch>
{
    public void Configure(EntityTypeBuilder<StockBatch> builder)
    {
        builder.ToTable("stock_batch");
        builder.HasKey(x => x.BatchId);
        builder.Property(x => x.BatchId).HasColumnName("batch_id").ValueGeneratedOnAdd();
        builder.Property(x => x.StockId).HasColumnName("stock_id").IsRequired();
        builder.Property(x => x.BatchNo).HasColumnName("batch_no").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReceivedQty).HasColumnName("received_qty").HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.AvailableQty).HasColumnName("available_qty").HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
        builder.Property(x => x.ReceivedDate).HasColumnName("received_date").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.StockInventory).WithMany(x => x.Batches).HasForeignKey(x => x.StockId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movement");
        builder.HasKey(x => x.MovementId);
        builder.Property(x => x.MovementId).HasColumnName("movement_id").ValueGeneratedOnAdd();
        builder.Property(x => x.BatchId).HasColumnName("batch_id").IsRequired();
        builder.Property(x => x.StockId).HasColumnName("stock_id").IsRequired();
        builder.Property(x => x.MovementType).HasColumnName("movement_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ReferenceNo).HasColumnName("reference_no").HasMaxLength(50);
        builder.Property(x => x.Qty).HasColumnName("qty").HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.PreviousQty).HasColumnName("previous_qty").HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.NewQty).HasColumnName("new_qty").HasColumnType("decimal(18,3)").IsRequired();
        builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(50).IsRequired();
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ReferenceType).HasColumnName("reference_type").HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasOne(x => x.StockBatch).WithMany(x => x.Movements).HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StockInventory).WithMany(x => x.Movements).HasForeignKey(x => x.StockId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    }
}
