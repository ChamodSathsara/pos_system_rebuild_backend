using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IVendorLedgerRepository : IGenericRepository<VendorLedger, int>
{
    Task<VendorLedger?> GetByVendorIdAsync(int vendorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VendorLedger>> GetAllWithVendorAsync(CancellationToken cancellationToken = default);

    Task<VendorLedger?> GetByIdWithVendorAsync(int ledgerId, CancellationToken cancellationToken = default);
}
