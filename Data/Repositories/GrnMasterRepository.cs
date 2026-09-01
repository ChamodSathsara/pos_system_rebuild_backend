using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class GrnMasterRepository : GenericRepository<GrnMaster, int>, IGrnMasterRepository
{
    private const string CodePrefix = "GRN";

    public GrnMasterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> GrnNoExistsAsync(string grnNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(g => g.GrnNo == grnNo, cancellationToken);
    }

    public async Task<string> GenerateNextGrnNoAsync(CancellationToken cancellationToken = default)
    {
        var lastCode = await DbSet
            .AsNoTracking()
            .Where(g => g.GrnNo != null && g.GrnNo.StartsWith(CodePrefix))
            .OrderByDescending(g => g.GrnNo)
            .Select(g => g.GrnNo)
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

    public async Task<GrnMaster?> GetByIdWithDetailsAsync(int grnId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(g => g.Vendor)
            .Include(g => g.Branch)
            .Include(g => g.Warehouse)
            .Include(g => g.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(g => g.GrnId == grnId, cancellationToken);
    }

    public async Task<IReadOnlyList<GrnMaster>> SearchAsync(
        string? poNo,
        int? vendorId,
        string? branchCode,
        string? warehouseCode,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(g => g.Vendor).AsQueryable();

        if (!string.IsNullOrWhiteSpace(poNo))
        {
            query = query.Where(g => g.PoNo == poNo);
        }

        if (vendorId.HasValue)
        {
            query = query.Where(g => g.VendorId == vendorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(branchCode))
        {
            query = query.Where(g => g.BranchCode == branchCode);
        }

        if (!string.IsNullOrWhiteSpace(warehouseCode))
        {
            query = query.Where(g => g.WarehouseCode == warehouseCode);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(g => g.GrnDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(g => g.GrnDate <= toDate.Value);
        }

        return await query.OrderByDescending(g => g.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<bool> HasReturnsAsync(int grnId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<GrnReturn>().AsNoTracking().AnyAsync(r => r.GrnId == grnId, cancellationToken);
    }
}
