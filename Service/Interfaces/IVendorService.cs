using PosApi.DTOs.Vendor;

namespace PosApi.Service.Interfaces;

public interface IVendorService
{
    Task<IReadOnlyList<VendorDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<VendorDto> GetByIdAsync(int vendorId, CancellationToken cancellationToken = default);
    Task<VendorDto> GetByCodeAsync(string vendorCode, CancellationToken cancellationToken = default);
    Task<VendorDto> CreateAsync(CreateVendorDto request, CancellationToken cancellationToken = default);
    Task<VendorDto> UpdateAsync(int vendorId, UpdateVendorDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int vendorId, CancellationToken cancellationToken = default);
}
