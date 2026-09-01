using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class SaleReturnRepository : GenericRepository<SaleReturn, string>, ISaleReturnRepository
{
    private const string CodePrefix = "SRT";

    public SaleReturnRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ReturnNoExistsAsync(string returnNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(r => r.ReturnNo == returnNo, cancellationToken);
    }

    public async Task<string> GenerateNextReturnNoAsync(CancellationToken cancellationToken = default)
    {
        var lastCode = await DbSet
            .AsNoTracking()
            .Where(r => r.ReturnNo.StartsWith(CodePrefix))
            .OrderByDescending(r => r.ReturnNo)
            .Select(r => r.ReturnNo)
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

    public async Task<SaleReturn?> GetByIdWithDetailsAsync(string returnNo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Sale)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.ReturnNo == returnNo, cancellationToken);
    }

    public async Task<IReadOnlyList<SaleReturn>> SearchAsync(
        string? invoiceNo,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(r => r.Sale).AsQueryable();

        if (!string.IsNullOrWhiteSpace(invoiceNo))
        {
            query = query.Where(r => r.InvoiceNo == invoiceNo);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.ReturnDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.ReturnDate <= toDate.Value);
        }

        return await query.OrderByDescending(r => r.ReturnDate).ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetReturnedQuantityForItemAsync(string invoiceNo, string itemCode, CancellationToken cancellationToken = default)
    {
        return await Context.Set<SaleReturnItem>()
            .AsNoTracking()
            .Where(i => i.ItemCode == itemCode && i.SaleReturn != null && i.SaleReturn.InvoiceNo == invoiceNo)
            .SumAsync(i => i.Quantity ?? 0, cancellationToken);
    }
}
