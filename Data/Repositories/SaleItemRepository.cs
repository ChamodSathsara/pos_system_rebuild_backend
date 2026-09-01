using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class SaleItemRepository : GenericRepository<SaleItem, int>, ISaleItemRepository
{
    public SaleItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SaleItem>> GetByInvoiceNoAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.InvoiceNo == invoiceNo)
            .ToListAsync(cancellationToken);
    }

    public async Task<SaleItem?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Sale)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }
}
