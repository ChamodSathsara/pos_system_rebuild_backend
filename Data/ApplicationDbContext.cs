using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;

namespace PosApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Organization
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    // Security (user_group replaced by user_role)
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRolePermission> UserRolePermissions => Set<UserRolePermission>();
    public DbSet<SystemUser> SystemUsers => Set<SystemUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    // Product
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<TaxMaster> TaxMasters => Set<TaxMaster>();
    public DbSet<ProductMaster> Products => Set<ProductMaster>();

    // Customer
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CreditCustomer> CreditCustomers => Set<CreditCustomer>();
    public DbSet<ChequeRegister> ChequeRegisters => Set<ChequeRegister>();

    // Vendor
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<VendorLedger> VendorLedgers => Set<VendorLedger>();

    // Purchase order
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<PurchaseOrderHistory> PurchaseOrderHistories => Set<PurchaseOrderHistory>();
    public DbSet<PurchaseOrderHistoryChange> PurchaseOrderHistoryChanges => Set<PurchaseOrderHistoryChange>();

    // GRN
    public DbSet<GrnMaster> GrnMasters => Set<GrnMaster>();
    public DbSet<GrnItem> GrnItems => Set<GrnItem>();
    public DbSet<GrnReturn> GrnReturns => Set<GrnReturn>();
    public DbSet<GrnReturnItem> GrnReturnItems => Set<GrnReturnItem>();

    // Stock
    public DbSet<StockInventory> StockInventories => Set<StockInventory>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // Damage
    public DbSet<DamageItem> DamageItems => Set<DamageItem>();

    // Discount
    public DbSet<Discount> Discounts => Set<Discount>();

    // Sales
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
    public DbSet<SaleReturnItem> SaleReturnItems => Set<SaleReturnItem>();

    // Payment
    public DbSet<Payment> Payments => Set<Payment>();

    // Expense
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();

    // Cashier Shift (Day/Shift cash closing)
    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<CashierShiftHistory> CashierShiftHistories => Set<CashierShiftHistory>();

    // Logging
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ItemLog> ItemLogs => Set<ItemLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasSequence<int>("customer_code_sequence");

        // Applies every IEntityTypeConfiguration<T> found in Data/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
