using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class VendorLedgerRepository : GenericRepository<VendorLedger, int>, IVendorLedgerRepository
{
    public VendorLedgerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<VendorLedger?> GetByVendorIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(l => l.VendorId == vendorId, cancellationToken);
    }

    public async Task<IReadOnlyList<VendorLedger>> GetAllWithVendorAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Include(l => l.Vendor).ToListAsync(cancellationToken);
    }

    public async Task<VendorLedger?> GetByIdWithVendorAsync(int ledgerId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Include(l => l.Vendor).FirstOrDefaultAsync(l => l.LedgerId == ledgerId, cancellationToken);
    }
}
