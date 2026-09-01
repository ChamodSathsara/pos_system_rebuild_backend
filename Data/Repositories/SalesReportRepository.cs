using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class SalesReportRepository : ISalesReportRepository
{
    private readonly ApplicationDbContext _context;

    public SalesReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Sale>> GetSalesForReportAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        string? itemCode,
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Sale>()
            .AsNoTracking()
            .Include(s => s.Branch)
            .Include(s => s.Customer)
            .Include(s => s.CreatedByUser)
            .Include(s => s.Payments)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Where(s => s.Status != SaleStatus.Cancelled)
            .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(s => s.BranchCode == branchCode);
        }

        if (!string.IsNullOrWhiteSpace(cashierCode))
        {
            query = query.Where(s => s.CreatedBy == cashierCode);
        }

        if (!string.IsNullOrWhiteSpace(customerCode))
        {
            query = query.Where(s => s.CustomerCode == customerCode);
        }

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(s => s.Items.Any(i => i.ItemCode == itemCode));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(s => s.Items.Any(i => i.Product != null && i.Product.CategoryId == categoryId.Value));
        }

        return await query.OrderBy(s => s.SaleDate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SaleReturn>> GetSaleReturnsForReportAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        string? itemCode,
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<SaleReturn>()
            .AsNoTracking()
            .Include(r => r.Sale).ThenInclude(s => s!.Branch)
            .Include(r => r.Sale).ThenInclude(s => s!.Customer)
            .Include(r => r.CreatedByUser)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .Where(r => r.ReturnDate >= fromDate && r.ReturnDate <= toDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(r => r.Sale != null && r.Sale.BranchCode == branchCode);
        }

        if (!string.IsNullOrWhiteSpace(cashierCode))
        {
            // "Cashier" here means whoever processed the return itself, not necessarily the
            // cashier of the original sale.
            query = query.Where(r => r.CreatedBy == cashierCode);
        }

        if (!string.IsNullOrWhiteSpace(customerCode))
        {
            query = query.Where(r => r.Sale != null && r.Sale.CustomerCode == customerCode);
        }

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(r => r.Items.Any(i => i.ItemCode == itemCode));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(r => r.Items.Any(i => i.Product != null && i.Product.CategoryId == categoryId.Value));
        }

        return await query.OrderBy(r => r.ReturnDate).ToListAsync(cancellationToken);
    }
}
