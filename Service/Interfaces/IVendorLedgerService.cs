using PosApi.DTOs.Vendor;

namespace PosApi.Service.Interfaces;

public interface IVendorLedgerService
{
    Task<IReadOnlyList<VendorLedgerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<VendorLedgerDto> GetByIdAsync(int ledgerId, CancellationToken cancellationToken = default);
    Task<VendorLedgerDto> GetByVendorIdAsync(int vendorId, CancellationToken cancellationToken = default);
    Task<VendorLedgerDto> CreateAsync(CreateVendorLedgerDto request, CancellationToken cancellationToken = default);
    Task<VendorLedgerDto> UpdateAsync(int ledgerId, UpdateVendorLedgerDto request, CancellationToken cancellationToken = default);

    /// <summary>Records a payment made to the vendor, incrementing PaidCredit and recomputing the outstanding balance.</summary>
    Task<VendorLedgerDto> RecordPaymentAsync(int vendorId, RecordVendorPaymentDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int ledgerId, CancellationToken cancellationToken = default);
}
