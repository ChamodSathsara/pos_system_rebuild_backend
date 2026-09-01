using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IVendorRepository : IGenericRepository<Vendor, int>
{
    Task<bool> VendorCodeExistsAsync(string vendorCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the next sequential vendor code (e.g. "VEN00001", "VEN00002", ...) for use when
    /// the caller does not supply one explicitly.
    /// </summary>
    Task<string> GenerateNextVendorCodeAsync(CancellationToken cancellationToken = default);

    Task<Vendor?> GetByCodeAsync(string vendorCode, CancellationToken cancellationToken = default);

    Task<Vendor?> GetByIdWithLedgerAsync(int vendorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vendor>> GetAllWithLedgerAsync(bool? isActive, CancellationToken cancellationToken = default);

    Task<bool> HasPurchaseOrdersAsync(int vendorId, CancellationToken cancellationToken = default);

    Task<bool> HasGrnsAsync(int vendorId, CancellationToken cancellationToken = default);
}
