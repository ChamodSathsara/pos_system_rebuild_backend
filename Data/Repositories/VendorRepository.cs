using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class VendorRepository : GenericRepository<Vendor, int>, IVendorRepository
{
    private const string CodePrefix = "VEN";

    public VendorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> VendorCodeExistsAsync(string vendorCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(v => v.VendorCode == vendorCode, cancellationToken);
    }

    public async Task<string> GenerateNextVendorCodeAsync(CancellationToken cancellationToken = default)
    {
        var lastCode = await DbSet
            .AsNoTracking()
            .Where(v => v.VendorCode.StartsWith(CodePrefix))
            .OrderByDescending(v => v.VendorCode)
            .Select(v => v.VendorCode)
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

        return $"{CodePrefix}{nextSequence:D5}";
    }

    public async Task<Vendor?> GetByCodeAsync(string vendorCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(v => v.VendorCode == vendorCode, cancellationToken);
    }

    public async Task<Vendor?> GetByIdWithLedgerAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(v => v.VendorLedger)
            .FirstOrDefaultAsync(v => v.VendorId == vendorId, cancellationToken);
    }

    public async Task<IReadOnlyList<Vendor>> GetAllWithLedgerAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(v => v.VendorLedger).AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(v => v.IsActive == isActive.Value);
        }

        return await query.OrderBy(v => v.VendorName).ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPurchaseOrdersAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<PurchaseOrder>().AsNoTracking().AnyAsync(po => po.VendorId == vendorId, cancellationToken);
    }

    public async Task<bool> HasGrnsAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<GrnMaster>().AsNoTracking().AnyAsync(g => g.VendorId == vendorId, cancellationToken);
    }
}
