using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customer");
        builder.HasKey(x => x.CustomerCode);
        builder.Property(x => x.CustomerCode).HasColumnName("customer_code").HasMaxLength(50);
        builder.Property(x => x.CustomerName).HasColumnName("customer_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Mobile).HasColumnName("mobile").HasMaxLength(20);
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(255);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
        builder.Property(x => x.CustomerType).HasColumnName("customer_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.LoyaltyPoints).HasColumnName("loyalty_points").IsRequired();
        builder.Property(x => x.CreditLimit).HasColumnName("credit_limit").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.Email);
    }
}

public class CreditCustomerConfiguration : IEntityTypeConfiguration<CreditCustomer>
{
    public void Configure(EntityTypeBuilder<CreditCustomer> builder)
    {
        builder.ToTable("credit_customer");
        builder.HasKey(x => x.CreditId);
        builder.Property(x => x.CreditId).HasColumnName("credit_id").ValueGeneratedNever();
        builder.Property(x => x.CustomerCode).HasColumnName("customer_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreditLimit).HasColumnName("credit_limit").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ReceiptTotal).HasColumnName("receipt_total").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ReturnTotal).HasColumnName("return_total").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.PaidCredit).HasColumnName("paid_credit").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Outstanding).HasColumnName("outstanding").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.IsActivate).HasColumnName("is_activate").IsRequired();

        builder.HasIndex(x => x.CustomerCode).IsUnique();

        builder.HasOne(x => x.Customer)
            .WithOne(x => x.CreditCustomer)
            .HasForeignKey<CreditCustomer>(x => x.CustomerCode)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChequeRegisterConfiguration : IEntityTypeConfiguration<ChequeRegister>
{
    public void Configure(EntityTypeBuilder<ChequeRegister> builder)
    {
        builder.ToTable("cheque_register");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ChequeNo).HasColumnName("cheque_no").HasMaxLength(255).IsRequired();
        builder.Property(x => x.CustomerCode).HasColumnName("customer_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReceiptNo).HasColumnName("recipt_no").HasMaxLength(255);
        builder.Property(x => x.ChequeDate).HasColumnName("cheque_date").IsRequired();
        builder.Property(x => x.PaidDate).HasColumnName("paid_date");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.ChequeRegisters)
            .HasForeignKey(x => x.CustomerCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
