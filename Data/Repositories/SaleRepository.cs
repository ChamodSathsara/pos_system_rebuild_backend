using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class SaleRepository : GenericRepository<Sale, string>, ISaleRepository
{
    private const string CodePrefix = "INV";

    public SaleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> InvoiceNoExistsAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(s => s.InvoiceNo == invoiceNo, cancellationToken);
    }

    public async Task<string> GenerateNextInvoiceNoAsync(CancellationToken cancellationToken = default)
    {
        var lastCode = await DbSet
            .AsNoTracking()
            .Where(s => s.InvoiceNo.StartsWith(CodePrefix))
            .OrderByDescending(s => s.InvoiceNo)
            .Select(s => s.InvoiceNo)
            .FirstOrDefaultAsync(cancellationToken);

        var nextSequence = 1;
        if (lastCode is not null && lastCode.Length > CodePrefix.Length)
        {
            var numericPart = lastCode[CodePrefix.Length..];
            if (int.TryParse(numericPart, out var parsed))
            {
                nextSequence = parsed + 1;
            }
        }

        return $"{CodePrefix}{nextSequence:D6}";
    }

    public async Task<Sale?> GetByIdWithDetailsAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Branch).ThenInclude(b => b!.Company)
            .Include(s => s.Customer)
            .Include(s => s.CreatedByUser)
            .Include(s => s.Payments)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.InvoiceNo == invoiceNo, cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> SearchAsync(
        string? branchCode,
        string? customerCode,
        SaleStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(s => s.Customer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(s => s.BranchCode == branchCode);
        }

        if (!string.IsNullOrWhiteSpace(customerCode))
        {
            query = query.Where(s => s.CustomerCode == customerCode);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.SaleDate <= toDate.Value);
        }

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<bool> HasReturnsAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<SaleReturn>().AsNoTracking().AnyAsync(r => r.InvoiceNo == invoiceNo, cancellationToken);
    }
}
