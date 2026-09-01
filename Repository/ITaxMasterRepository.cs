using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface ITaxMasterRepository : IGenericRepository<TaxMaster, string>
{
    Task<bool> TaxCodeExistsAsync(string taxCode, CancellationToken cancellationToken = default);

    Task<bool> HasProductsAsync(string taxCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaxMaster>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default);
}
