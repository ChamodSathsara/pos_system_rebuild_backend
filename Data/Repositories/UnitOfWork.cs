using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(
        ApplicationDbContext context,
        IUserRepository users,
        ICustomerRepository customers,
        IRefreshTokenRepository refreshTokens,
        ICompanyRepository companies,
        IBranchRepository branches,
        IWarehouseRepository warehouses,
        IUserRoleRepository userRoles,
        IPermissionRepository permissions,
        IUserRolePermissionRepository userRolePermissions,
        IVendorRepository vendors,
        IVendorLedgerRepository vendorLedgers,
        IStockInventoryRepository stockInventories,
        IStockBatchRepository stockBatches,
        IStockMovementRepository stockMovements,
        ICategoryRepository categories,
        IBrandRepository brands,
        ITaxMasterRepository taxMasters,
        IProductRepository products,
        IPurchaseOrderRepository purchaseOrders,
        IPurchaseOrderItemRepository purchaseOrderItems,
        IPurchaseOrderHistoryRepository purchaseOrderHistories,
        IPurchaseOrderHistoryChangeRepository purchaseOrderHistoryChanges,
        IGrnMasterRepository grnMasters,
        IGrnItemRepository grnItems,
        IGrnReturnRepository grnReturns,
        IGrnReturnItemRepository grnReturnItems,
        IDiscountRepository discounts,
        ISaleRepository sales,
        ISaleItemRepository saleItems,
        ISaleReturnRepository saleReturns,
        ISaleReturnItemRepository saleReturnItems,
        IPaymentRepository payments,
        IExpenseCategoryRepository expenseCategories,
        IExpenseRepository expenses,
        IDamageItemRepository damageItems,
        IItemLogRepository itemLogs,
        ICashierShiftRepository cashierShifts,
        ICashierShiftHistoryRepository cashierShiftHistories,
        ISalesReportRepository salesReports)
    {
        _context = context;
        Users = users;
        Customers = customers;
        RefreshTokens = refreshTokens;
        Companies = companies;
        Branches = branches;
        Warehouses = warehouses;
        UserRoles = userRoles;
        Permissions = permissions;
        UserRolePermissions = userRolePermissions;
        Vendors = vendors;
        VendorLedgers = vendorLedgers;
        StockInventories = stockInventories;
        StockBatches = stockBatches;
        StockMovements = stockMovements;
        Categories = categories;
        Brands = brands;
        TaxMasters = taxMasters;
        Products = products;
        PurchaseOrders = purchaseOrders;
        PurchaseOrderItems = purchaseOrderItems;
        PurchaseOrderHistories = purchaseOrderHistories;
        PurchaseOrderHistoryChanges = purchaseOrderHistoryChanges;
        GrnMasters = grnMasters;
        GrnItems = grnItems;
        GrnReturns = grnReturns;
        GrnReturnItems = grnReturnItems;
        Discounts = discounts;
        Sales = sales;
        SaleItems = saleItems;
        SaleReturns = saleReturns;
        SaleReturnItems = saleReturnItems;
        Payments = payments;
        ExpenseCategories = expenseCategories;
        Expenses = expenses;
        DamageItems = damageItems;
        ItemLogs = itemLogs;
        CashierShifts = cashierShifts;
        CashierShiftHistories = cashierShiftHistories;
        SalesReports = salesReports;
    }

    public IUserRepository Users { get; }
    public ICustomerRepository Customers { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public ICompanyRepository Companies { get; }
    public IBranchRepository Branches { get; }
    public IWarehouseRepository Warehouses { get; }
    public IUserRoleRepository UserRoles { get; }
    public IPermissionRepository Permissions { get; }
    public IUserRolePermissionRepository UserRolePermissions { get; }
    public IVendorRepository Vendors { get; }
    public IVendorLedgerRepository VendorLedgers { get; }
    public IStockInventoryRepository StockInventories { get; }
    public IStockBatchRepository StockBatches { get; }
    public IStockMovementRepository StockMovements { get; }
    public ICategoryRepository Categories { get; }
    public IBrandRepository Brands { get; }
    public ITaxMasterRepository TaxMasters { get; }
    public IProductRepository Products { get; }
    public IPurchaseOrderRepository PurchaseOrders { get; }
    public IPurchaseOrderItemRepository PurchaseOrderItems { get; }
    public IPurchaseOrderHistoryRepository PurchaseOrderHistories { get; }
    public IPurchaseOrderHistoryChangeRepository PurchaseOrderHistoryChanges { get; }
    public IGrnMasterRepository GrnMasters { get; }
    public IGrnItemRepository GrnItems { get; }
    public IGrnReturnRepository GrnReturns { get; }
    public IGrnReturnItemRepository GrnReturnItems { get; }
    public IDiscountRepository Discounts { get; }
    public ISaleRepository Sales { get; }
    public ISaleItemRepository SaleItems { get; }
    public ISaleReturnRepository SaleReturns { get; }
    public ISaleReturnItemRepository SaleReturnItems { get; }
    public IPaymentRepository Payments { get; }
    public IExpenseCategoryRepository ExpenseCategories { get; }
    public IExpenseRepository Expenses { get; }
    public IDamageItemRepository DamageItems { get; }
    public IItemLogRepository ItemLogs { get; }
    public ICashierShiftRepository CashierShifts { get; }
    public ICashierShiftHistoryRepository CashierShiftHistories { get; }
    public ISalesReportRepository SalesReports { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}