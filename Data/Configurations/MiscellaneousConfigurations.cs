using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class DamageItemConfiguration : IEntityTypeConfiguration<DamageItem>
{
    public void Configure(EntityTypeBuilder<DamageItem> builder)
    {
        builder.ToTable("damage_item");
        builder.HasKey(x => x.DamageId);
        builder.Property(x => x.DamageId).HasColumnName("damage_id").ValueGeneratedOnAdd();
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50);
        builder.Property(x => x.WarehouseCode).HasColumnName("warehouse_code").HasMaxLength(50);
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,3)");
        builder.Property(x => x.CostAmount).HasColumnName("cost_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(255);
        builder.Property(x => x.DamageDate).HasColumnName("damage_date");
        builder.Property(x => x.ReportedBy).HasColumnName("reported_by").HasMaxLength(50);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReportedByUser).WithMany().HasForeignKey(x => x.ReportedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("discount");
        builder.HasKey(x => x.DiscountCode);
        builder.Property(x => x.DiscountCode).HasColumnName("discount_code").HasMaxLength(50);
        builder.Property(x => x.DiscountName).HasColumnName("discount_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DiscountType).HasColumnName("discount_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.DiscountMethod).HasColumnName("discount_method").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.DiscountValue).HasColumnName("discount_value").HasColumnType("decimal(10,2)");
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.MinQuantity).HasColumnName("min_quantity").HasColumnType("decimal(18,3)");
        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.StartTime).HasColumnName("start_time");
        builder.Property(x => x.EndTime).HasColumnName("end_time");
        builder.Property(x => x.MinBillAmount).HasColumnName("min_bill_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.ApplicableTo).HasColumnName("applicable_to").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sale");
        builder.HasKey(x => x.InvoiceNo);
        builder.Property(x => x.InvoiceNo).HasColumnName("invoice_no").HasMaxLength(50);
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50);
        builder.Property(x => x.CustomerCode).HasColumnName("customer_code").HasMaxLength(50);
        builder.Property(x => x.SaleDate).HasColumnName("sale_date");
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.PaidAmount).HasColumnName("paid_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.BalanceAmount).HasColumnName("balance_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Customer).WithMany(x => x.Sales).HasForeignKey(x => x.CustomerCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_item");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.InvoiceNo).HasColumnName("invoice_no").HasMaxLength(50);
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalPrice).HasColumnName("total_price").HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Sale).WithMany(x => x.Items).HasForeignKey(x => x.InvoiceNo).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SaleReturnConfiguration : IEntityTypeConfiguration<SaleReturn>
{
    public void Configure(EntityTypeBuilder<SaleReturn> builder)
    {
        builder.ToTable("sale_return");
        builder.HasKey(x => x.ReturnNo);
        builder.Property(x => x.ReturnNo).HasColumnName("return_no").HasMaxLength(50);
        builder.Property(x => x.InvoiceNo).HasColumnName("invoice_no").HasMaxLength(50);
        builder.Property(x => x.ReturnDate).HasColumnName("return_date");
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(255);
        builder.Property(x => x.TotalReturnAmount).HasColumnName("total_return_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(50);

        builder.HasOne(x => x.Sale).WithMany(x => x.Returns).HasForeignKey(x => x.InvoiceNo).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SaleReturnItemConfiguration : IEntityTypeConfiguration<SaleReturnItem>
{
    public void Configure(EntityTypeBuilder<SaleReturnItem> builder)
    {
        builder.ToTable("sale_return_item");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.ReturnNo).HasColumnName("return_no").HasMaxLength(50);
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.SaleReturn).WithMany(x => x.Items).HasForeignKey(x => x.ReturnNo).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payment");
        builder.HasKey(x => x.PaymentId);
        builder.Property(x => x.PaymentId).HasColumnName("payment_id").ValueGeneratedOnAdd();
        builder.Property(x => x.InvoiceNo).HasColumnName("invoice_no").HasMaxLength(50);
        builder.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.PaymentDate).HasColumnName("payment_date");
        builder.Property(x => x.ReferenceNo).HasColumnName("reference_no").HasMaxLength(100);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ReceivedBy).HasColumnName("received_by").HasMaxLength(50);

        builder.HasOne(x => x.Sale).WithMany(x => x.Payments).HasForeignKey(x => x.InvoiceNo).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReceivedByUser).WithMany().HasForeignKey(x => x.ReceivedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("expense_category");
        builder.HasKey(x => x.CategoryId);
        builder.Property(x => x.CategoryId).HasColumnName("category_id").ValueGeneratedOnAdd();
        builder.Property(x => x.CategoryName).HasColumnName("category_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
    }
}

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expense");
        builder.HasKey(x => x.ExpenseId);
        builder.Property(x => x.ExpenseId).HasColumnName("expense_id").ValueGeneratedOnAdd();
        builder.Property(x => x.DamageId).HasColumnName("damage_id");
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50);
        builder.Property(x => x.CategoryId).HasColumnName("category_id");
        builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.ExpenseDate).HasColumnName("expense_date");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(x => x.PaidBy).HasColumnName("paid_by").HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Category).WithMany(x => x.Expenses).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PaidByUser).WithMany().HasForeignKey(x => x.PaidBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DamageItem)
            .WithOne(x => x.Expense)
            .HasForeignKey<Expense>(x => x.DamageId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.DamageId).IsUnique();
    }
}

public class CashierShiftConfiguration : IEntityTypeConfiguration<CashierShift>
{
    public void Configure(EntityTypeBuilder<CashierShift> builder)
    {
        builder.ToTable("cashier_shift");
        builder.HasKey(x => x.ShiftId);
        builder.Property(x => x.ShiftId).HasColumnName("shift_id").ValueGeneratedOnAdd();
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50);
        builder.Property(x => x.CashierCode).HasColumnName("cashier_code").HasMaxLength(50);
        builder.Property(x => x.OpeningCash).HasColumnName("opening_cash").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.OpenedAt).HasColumnName("opened_at").IsRequired();
        builder.Property(x => x.ExpectedCash).HasColumnName("expected_cash").HasColumnType("decimal(18,2)");
        builder.Property(x => x.ActualCash).HasColumnName("actual_cash").HasColumnType("decimal(18,2)");
        builder.Property(x => x.DifferenceAmount).HasColumnName("difference_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReasonType).HasColumnName("reason_type").HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ReasonDescription).HasColumnName("reason_description").HasMaxLength(500);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ClosedBy).HasColumnName("closed_by").HasMaxLength(50);
        builder.Property(x => x.ClosedAt).HasColumnName("closed_at");

        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Cashier).WithMany().HasForeignKey(x => x.CashierCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ClosedByUser).WithMany().HasForeignKey(x => x.ClosedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CashierShiftHistoryConfiguration : IEntityTypeConfiguration<CashierShiftHistory>
{
    public void Configure(EntityTypeBuilder<CashierShiftHistory> builder)
    {
        builder.ToTable("cashier_shift_history");
        builder.HasKey(x => x.HistoryId);
        builder.Property(x => x.HistoryId).HasColumnName("history_id").ValueGeneratedOnAdd();
        builder.Property(x => x.ShiftId).HasColumnName("shift_id");
        builder.Property(x => x.Action).HasColumnName("action").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.ExpectedCash).HasColumnName("expected_cash").HasColumnType("decimal(18,2)");
        builder.Property(x => x.ActualCash).HasColumnName("actual_cash").HasColumnType("decimal(18,2)");
        builder.Property(x => x.DifferenceAmount).HasColumnName("difference_amount").HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReasonType).HasColumnName("reason_type").HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ReasonDescription).HasColumnName("reason_description").HasMaxLength(500);
        builder.Property(x => x.ChangedBy).HasColumnName("changed_by").HasMaxLength(50);
        builder.Property(x => x.ChangedAt).HasColumnName("changed_at");
        builder.Property(x => x.Remarks).HasColumnName("remarks").HasMaxLength(255);

        builder.HasOne(x => x.Shift).WithMany(x => x.Histories).HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_log");
        builder.HasKey(x => x.LogId);
        builder.Property(x => x.LogId).HasColumnName("log_id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserCode).HasColumnName("user_code").HasMaxLength(50);
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(100);
        builder.Property(x => x.TableName).HasColumnName("table_name").HasMaxLength(50);
        builder.Property(x => x.RecordId).HasColumnName("record_id").HasMaxLength(50);
        builder.Property(x => x.OldValue).HasColumnName("old_value").HasMaxLength(255);
        builder.Property(x => x.NewValue).HasColumnName("new_value").HasMaxLength(255);
        builder.Property(x => x.ActionTime).HasColumnName("action_time");
    }
}

public class ItemLogConfiguration : IEntityTypeConfiguration<ItemLog>
{
    public void Configure(EntityTypeBuilder<ItemLog> builder)
    {
        builder.ToTable("item_log");
        builder.HasKey(x => x.LogId);
        builder.Property(x => x.LogId).HasColumnName("log_id").ValueGeneratedOnAdd();
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(100);
        builder.Property(x => x.OldValue).HasColumnName("old_value").HasMaxLength(255);
        builder.Property(x => x.NewValue).HasColumnName("new_value").HasMaxLength(255);
        builder.Property(x => x.ChangedBy).HasColumnName("changed_by").HasMaxLength(50);
        builder.Property(x => x.ChangedAt).HasColumnName("changed_at");

        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ItemCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedBy).OnDelete(DeleteBehavior.Restrict);
    }
}
