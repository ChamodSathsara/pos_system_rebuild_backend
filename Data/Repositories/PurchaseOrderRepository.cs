using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class PurchaseOrderRepository : GenericRepository<PurchaseOrder, string>, IPurchaseOrderRepository
{
    private const string CodePrefix = "PO";

    public PurchaseOrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> PoNoExistsAsync(string poNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(p => p.PoNo == poNo, cancellationToken);
    }

    public async Task<string> GenerateNextPoNoAsync(CancellationToken cancellationToken = default)
    {
        var lastCode = await DbSet
            .AsNoTracking()
            .Where(p => p.PoNo.StartsWith(CodePrefix))
            .OrderByDescending(p => p.PoNo)
            .Select(p => p.PoNo)
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

    public async Task<PurchaseOrder?> GetByIdWithItemsAsync(string poNo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PoNo == poNo, cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> SearchAsync(
        int? vendorId,
        string? branchCode,
        PurchaseOrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(p => p.Vendor).AsQueryable();

        if (vendorId.HasValue)
        {
            query = query.Where(p => p.VendorId == vendorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(p => p.BranchCode == branchCode);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.PoDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.PoDate <= toDate.Value);
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<bool> HasGrnsAsync(string poNo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<GrnMaster>().AsNoTracking().AnyAsync(g => g.PoNo == poNo, cancellationToken);
    }
}