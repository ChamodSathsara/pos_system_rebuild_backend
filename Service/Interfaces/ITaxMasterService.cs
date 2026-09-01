using PosApi.DTOs.Product;

namespace PosApi.Service.Interfaces;

public interface ITaxMasterService
{
    Task<IReadOnlyList<TaxMasterDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<TaxMasterDto> GetByCodeAsync(string taxCode, CancellationToken cancellationToken = default);
    Task<TaxMasterDto> CreateAsync(CreateTaxMasterDto request, CancellationToken cancellationToken = default);
    Task<TaxMasterDto> UpdateAsync(string taxCode, UpdateTaxMasterDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string taxCode, CancellationToken cancellationToken = default);
}
