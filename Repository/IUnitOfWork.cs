namespace PosApi.Repository;

/// <summary>
/// Aggregates repositories that participate in the same ApplicationDbContext instance so a
/// service can commit multiple repository operations atomically with a single SaveChanges call.
/// </summary>
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICustomerRepository Customers { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    // Organization
    ICompanyRepository Companies { get; }
    IBranchRepository Branches { get; }
    IWarehouseRepository Warehouses { get; }

    // Security
    IUserRoleRepository UserRoles { get; }
    IPermissionRepository Permissions { get; }
    IUserRolePermissionRepository UserRolePermissions { get; }

    // Vendor
    IVendorRepository Vendors { get; }
    IVendorLedgerRepository VendorLedgers { get; }

    // Stock
    IStockInventoryRepository StockInventories { get; }
    IStockBatchRepository StockBatches { get; }
    IStockMovementRepository StockMovements { get; }

    // Product
    ICategoryRepository Categories { get; }
    IBrandRepository Brands { get; }
    ITaxMasterRepository TaxMasters { get; }
    IProductRepository Products { get; }

    // Purchase order
    IPurchaseOrderRepository PurchaseOrders { get; }
    IPurchaseOrderItemRepository PurchaseOrderItems { get; }
    IPurchaseOrderHistoryRepository PurchaseOrderHistories { get; }
    IPurchaseOrderHistoryChangeRepository PurchaseOrderHistoryChanges { get; }

    // GRN
    IGrnMasterRepository GrnMasters { get; }
    IGrnItemRepository GrnItems { get; }
    IGrnReturnRepository GrnReturns { get; }
    IGrnReturnItemRepository GrnReturnItems { get; }

    // Discount
    IDiscountRepository Discounts { get; }

    // Sales
    ISaleRepository Sales { get; }
    ISaleItemRepository SaleItems { get; }
    ISaleReturnRepository SaleReturns { get; }
    ISaleReturnItemRepository SaleReturnItems { get; }

    // Payment
    IPaymentRepository Payments { get; }

    // Expense
    IExpenseCategoryRepository ExpenseCategories { get; }
    IExpenseRepository Expenses { get; }

    // Miscellaneous
    IDamageItemRepository DamageItems { get; }
    IItemLogRepository ItemLogs { get; }

    // Cashier Shift (Day/Shift cash closing)
    ICashierShiftRepository CashierShifts { get; }
    ICashierShiftHistoryRepository CashierShiftHistories { get; }

    // Reports
    ISalesReportRepository SalesReports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}