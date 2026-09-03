using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class OperationalReportRepository
    : IOperationalReportRepository
{
    private readonly ApplicationDbContext _context;

    public OperationalReportRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StockInventory>>
        GetCurrentStockAsync(
            string? branchCode,
            string? warehouseCode,
            string? itemCode,
            int? categoryId,
            bool onlyAvailable,
            bool onlyBelowReorderLevel,
            CancellationToken cancellationToken = default)
    {
        var query = _context.Set<StockInventory>()
            .AsNoTracking()
            .Include(x => x.Product)
                .ThenInclude(x => x!.Category)
            .Include(x => x.Product)
                .ThenInclude(x => x!.Brand)
            .Include(x => x.Batches)
            .Where(x => x.Product != null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(x =>
                x.BranchCode == branchCode);
        }

        if (!string.IsNullOrWhiteSpace(warehouseCode))
        {
            query = query.Where(x =>
                x.WarehouseCode == warehouseCode);
        }

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(x =>
                x.ItemCode == itemCode);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.Product!.CategoryId == categoryId.Value);
        }

        if (onlyAvailable)
        {
            query = query.Where(x => x.CurrentQty > 0);
        }

        if (onlyBelowReorderLevel)
        {
            query = query.Where(x =>
                x.Product!.ReorderLevel.HasValue &&
                x.CurrentQty <= x.Product.ReorderLevel.Value);
        }

        return await query
            .OrderBy(x => x.ItemCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockMovement>>
        GetStockMovementsAsync(
            DateTime fromDate,
            DateTime toDate,
            string? branchCode,
            string? warehouseCode,
            string? itemCode,
            StockMovementType? movementType,
            StockReferenceType? referenceType,
            string? referenceNo,
            CancellationToken cancellationToken = default)
    {
        var query = _context.Set<StockMovement>()
            .AsNoTracking()
            .Include(x => x.StockInventory)
                .ThenInclude(x => x!.Product)
            .Include(x => x.StockBatch)
            .Include(x => x.CreatedByUser)
            .Where(x =>
                x.CreatedAt >= fromDate &&
                x.CreatedAt <= toDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(x =>
                x.StockInventory!.BranchCode == branchCode);
        }

        if (!string.IsNullOrWhiteSpace(warehouseCode))
        {
            query = query.Where(x =>
                x.StockInventory!.WarehouseCode == warehouseCode);
        }

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(x =>
                x.StockInventory!.ItemCode == itemCode);
        }

        if (movementType.HasValue)
        {
            query = query.Where(x =>
                x.MovementType == movementType.Value);
        }

        if (referenceType.HasValue)
        {
            query = query.Where(x =>
                x.ReferenceType == referenceType.Value);
        }

        if (!string.IsNullOrWhiteSpace(referenceNo))
        {
            query = query.Where(x =>
                x.ReferenceNo == referenceNo);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>>
        GetPurchaseOrdersAsync(
            DateTime fromDate,
            DateTime toDate,
            string? branchCode,
            int? vendorId,
            string? itemCode,
            PurchaseOrderStatus? status,
            CancellationToken cancellationToken = default)
    {
        var query = _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .Include(x => x.Vendor)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .Where(x =>
                x.PoDate >= fromDate &&
                x.PoDate <= toDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(x =>
                x.BranchCode == branchCode);
        }

        if (vendorId.HasValue)
        {
            query = query.Where(x =>
                x.VendorId == vendorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(x =>
                x.Items.Any(i => i.ItemCode == itemCode));
        }

        if (status.HasValue)
        {
            query = query.Where(x =>
                x.Status == status.Value);
        }
        else
        {
            query = query.Where(x =>
                x.Status != PurchaseOrderStatus.Cancelled);
        }

        return await query
            .OrderByDescending(x => x.PoDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GrnMaster>> GetGrnsAsync(
        string? branchCode,
        int? vendorId,
        string? itemCode,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<GrnMaster>()
            .AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(x =>
                x.BranchCode == branchCode);
        }

        if (vendorId.HasValue)
        {
            query = query.Where(x =>
                x.VendorId == vendorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(x =>
                x.Items.Any(i => i.ItemCode == itemCode));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GrnReturn>>
        GetGrnReturnsAsync(
            string? branchCode,
            int? vendorId,
            string? itemCode,
            CancellationToken cancellationToken = default)
    {
        var query = _context.Set<GrnReturn>()
            .AsNoTracking()
            .Include(x => x.GrnMaster)
            .Include(x => x.Items)
            .Where(x =>
                x.Status == GrnReturnStatus.Completed ||
                x.Status == GrnReturnStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(x =>
                x.GrnMaster != null &&
                x.GrnMaster.BranchCode == branchCode);
        }

        if (vendorId.HasValue)
        {
            query = query.Where(x =>
                x.GrnMaster != null &&
                x.GrnMaster.VendorId == vendorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(x =>
                x.Items.Any(i => i.ItemCode == itemCode));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Expense>> GetExpensesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        int? categoryId,
        string? paidBy,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Expense>()
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Category)
            .Include(x => x.PaidByUser)
            .Where(x =>
                x.ExpenseDate >= fromDate &&
                x.ExpenseDate <= toDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(x =>
                x.BranchCode == branchCode);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(paidBy))
        {
            query = query.Where(x =>
                x.PaidBy == paidBy);
        }

        return await query
            .OrderByDescending(x => x.ExpenseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetSalesAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Sale>()
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x =>
                x.Status == SaleStatus.Completed &&
                x.SaleDate >= fromDate &&
                x.SaleDate <= toDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(x =>
                x.BranchCode == branchCode);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SaleReturn>>
        GetSaleReturnsAsync(
            DateTime fromDate,
            DateTime toDate,
            string? branchCode,
            CancellationToken cancellationToken = default)
    {
        var query = _context.Set<SaleReturn>()
            .AsNoTracking()
            .Include(x => x.Sale)
            .Where(x =>
                x.ReturnDate >= fromDate &&
                x.ReturnDate <= toDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(x =>
                x.Sale != null &&
                x.Sale.BranchCode == branchCode);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockMovement>>
        GetProfitMovementsAsync(
            DateTime fromDate,
            DateTime toDate,
            string? branchCode,
            CancellationToken cancellationToken = default)
    {
        var query = _context.Set<StockMovement>()
            .AsNoTracking()
            .Include(x => x.StockInventory)
            .Where(x =>
                x.CreatedAt >= fromDate &&
                x.CreatedAt <= toDate &&
                (
                    x.ReferenceType == StockReferenceType.Sale ||
                    x.ReferenceType == StockReferenceType.SaleReturn
                ))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(x =>
                x.StockInventory!.BranchCode == branchCode);
        }

        return await query.ToListAsync(cancellationToken);
    }
}