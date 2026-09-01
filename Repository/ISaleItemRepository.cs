using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface ISaleItemRepository : IGenericRepository<SaleItem, int>
{
    Task<IReadOnlyList<SaleItem>> GetByInvoiceNoAsync(string invoiceNo, CancellationToken cancellationToken = default);

    Task<SaleItem?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
