using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class StockTransferRequestConfiguration : IEntityTypeConfiguration<StockTransferRequest>
{
    public void Configure(EntityTypeBuilder<StockTransferRequest> builder)
    {
        builder.ToTable("stock_transfer_request");
        builder.HasKey(x => x.TransferRequestId);
        builder.Property(x => x.TransferRequestId).HasColumnName("transfer_request_id").ValueGeneratedOnAdd();
        builder.Property(x => x.RequestNo).HasColumnName("request_no").HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.RequestNo).IsUnique();
        builder.Property(x => x.SourceWarehouseCode).HasColumnName("source_warehouse_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.DestinationWarehouseCode).HasColumnName("destination_warehouse_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.RequestDate).HasColumnName("request_date");
        builder.Property(x => x.RequiredDate).HasColumnName("required_date");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(500);
        builder.Property(x => x.RequestedBy).HasColumnName("requested_by").HasMaxLength(50);
        builder.Property(x => x.AcceptedBy).HasColumnName("accepted_by").HasMaxLength(50);
        builder.Property(x => x.AcceptedAt).HasColumnName("accepted_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => new { x.SourceWarehouseCode, x.Status, x.RequestDate });
        builder.HasIndex(x => new { x.DestinationWarehouseCode, x.Status, x.RequestDate });
        builder.HasOne(x => x.SourceWarehouse).WithMany().HasForeignKey(x => x.SourceWarehouseCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DestinationWarehouse).WithMany().HasForeignKey(x => x.DestinationWarehouseCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockTransferRequestLineConfiguration : IEntityTypeConfiguration<StockTransferRequestLine>
{
    public void Configure(EntityTypeBuilder<StockTransferRequestLine> builder)
    {
        builder.ToTable("stock_transfer_request_line"); builder.HasKey(x => x.TransferRequestLineId);
        builder.Property(x => x.TransferRequestLineId).HasColumnName("transfer_request_line_id").ValueGeneratedOnAdd();
        builder.Property(x => x.TransferRequestId).HasColumnName("transfer_request_id"); builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.RequestedQty).HasColumnName("requested_qty").HasColumnType("decimal(18,3)"); builder.Property(x => x.ApprovedQty).HasColumnName("approved_qty").HasColumnType("decimal(18,3)");
        builder.Property(x => x.DispatchedQty).HasColumnName("dispatched_qty").HasColumnType("decimal(18,3)"); builder.Property(x => x.ReceivedQty).HasColumnName("received_qty").HasColumnType("decimal(18,3)"); builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(500);
        builder.HasOne(x => x.TransferRequest).WithMany(x => x.Lines).HasForeignKey(x => x.TransferRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockTransferDispatchConfiguration : IEntityTypeConfiguration<StockTransferDispatch>
{
    public void Configure(EntityTypeBuilder<StockTransferDispatch> builder)
    {
        builder.ToTable("stock_transfer_dispatch"); builder.HasKey(x => x.DispatchId);
        builder.Property(x => x.DispatchId).HasColumnName("dispatch_id").ValueGeneratedOnAdd(); builder.Property(x => x.DispatchNo).HasColumnName("dispatch_no").HasMaxLength(50); builder.HasIndex(x => x.DispatchNo).IsUnique();
        builder.Property(x => x.TransferRequestId).HasColumnName("transfer_request_id"); builder.Property(x => x.VehicleNo).HasColumnName("vehicle_no").HasMaxLength(50); builder.Property(x => x.DriverName).HasColumnName("driver_name").HasMaxLength(100); builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(500); builder.Property(x => x.DispatchedBy).HasColumnName("dispatched_by").HasMaxLength(50); builder.Property(x => x.DispatchedAt).HasColumnName("dispatched_at");
        builder.HasOne(x => x.TransferRequest).WithMany(x => x.Dispatches).HasForeignKey(x => x.TransferRequestId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockTransferDispatchLineConfiguration : IEntityTypeConfiguration<StockTransferDispatchLine>
{
    public void Configure(EntityTypeBuilder<StockTransferDispatchLine> builder)
    {
        builder.ToTable("stock_transfer_dispatch_line"); builder.HasKey(x => x.DispatchLineId); builder.Property(x => x.DispatchLineId).HasColumnName("dispatch_line_id").ValueGeneratedOnAdd(); builder.Property(x => x.DispatchId).HasColumnName("dispatch_id"); builder.Property(x => x.TransferRequestLineId).HasColumnName("transfer_request_line_id"); builder.Property(x => x.BatchId).HasColumnName("batch_id"); builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,3)"); builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(18,2)");
        builder.HasOne(x => x.Dispatch).WithMany(x => x.Lines).HasForeignKey(x => x.DispatchId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.TransferRequestLine).WithMany().HasForeignKey(x => x.TransferRequestLineId).OnDelete(DeleteBehavior.Restrict); builder.HasOne(x => x.StockBatch).WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockTransferReceiptConfiguration : IEntityTypeConfiguration<StockTransferReceipt>
{
    public void Configure(EntityTypeBuilder<StockTransferReceipt> builder)
    {
        builder.ToTable("stock_transfer_receipt"); builder.HasKey(x => x.ReceiptId); builder.Property(x => x.ReceiptId).HasColumnName("receipt_id").ValueGeneratedOnAdd(); builder.Property(x => x.ReceiptNo).HasColumnName("receipt_no").HasMaxLength(50); builder.HasIndex(x => x.ReceiptNo).IsUnique(); builder.Property(x => x.DispatchId).HasColumnName("dispatch_id"); builder.HasIndex(x => x.DispatchId).IsUnique(); builder.Property(x => x.ReceivedBy).HasColumnName("received_by").HasMaxLength(50); builder.Property(x => x.ReceivedAt).HasColumnName("received_at"); builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(500); builder.HasOne(x => x.Dispatch).WithOne(x => x.Receipt).HasForeignKey<StockTransferReceipt>(x => x.DispatchId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockTransferReceiptLineConfiguration : IEntityTypeConfiguration<StockTransferReceiptLine>
{
    public void Configure(EntityTypeBuilder<StockTransferReceiptLine> builder)
    {
        builder.ToTable("stock_transfer_receipt_line"); builder.HasKey(x => x.ReceiptLineId); builder.Property(x => x.ReceiptLineId).HasColumnName("receipt_line_id").ValueGeneratedOnAdd(); builder.Property(x => x.ReceiptId).HasColumnName("receipt_id"); builder.Property(x => x.DispatchLineId).HasColumnName("dispatch_line_id"); builder.Property(x => x.ReceivedQty).HasColumnName("received_qty").HasColumnType("decimal(18,3)"); builder.Property(x => x.ShortQty).HasColumnName("short_qty").HasColumnType("decimal(18,3)"); builder.Property(x => x.DamagedQty).HasColumnName("damaged_qty").HasColumnType("decimal(18,3)"); builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(500); builder.HasOne(x => x.Receipt).WithMany(x => x.Lines).HasForeignKey(x => x.ReceiptId).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.DispatchLine).WithMany().HasForeignKey(x => x.DispatchLineId).OnDelete(DeleteBehavior.Restrict);
    }
}
