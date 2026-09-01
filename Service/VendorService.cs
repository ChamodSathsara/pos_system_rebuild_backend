using AutoMapper;
using PosApi.DTOs.Vendor;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class VendorService : IVendorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<VendorService> _logger;

    public VendorService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VendorService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VendorDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var vendors = await _unitOfWork.Vendors.GetAllWithLedgerAsync(isActive, cancellationToken);
        return _mapper.Map<IReadOnlyList<VendorDto>>(vendors);
    }

    public async Task<VendorDto> GetByIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdWithLedgerAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException("Vendor", vendorId);

        return _mapper.Map<VendorDto>(vendor);
    }

    public async Task<VendorDto> GetByCodeAsync(string vendorCode, CancellationToken cancellationToken = default)
    {
        var vendor = await _unitOfWork.Vendors.GetByCodeAsync(vendorCode, cancellationToken)
            ?? throw new NotFoundException("Vendor", vendorCode);

        return await GetByIdAsync(vendor.VendorId, cancellationToken);
    }

    public async Task<VendorDto> CreateAsync(CreateVendorDto request, CancellationToken cancellationToken = default)
    {
        var vendorCode = request.VendorCode?.Trim();

        if (string.IsNullOrWhiteSpace(vendorCode))
        {
            vendorCode = await _unitOfWork.Vendors.GenerateNextVendorCodeAsync(cancellationToken);
        }
        else if (await _unitOfWork.Vendors.VendorCodeExistsAsync(vendorCode, cancellationToken))
        {
            throw new ConflictException($"A vendor with code '{vendorCode}' already exists.");
        }

        var vendor = new Vendor
        {
            VendorCode = vendorCode,
            VendorName = request.VendorName.Trim(),
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            ContactPerson = request.ContactPerson,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Vendors.AddAsync(vendor, cancellationToken);

        // Every vendor is provisioned with a zeroed ledger up-front so downstream GRN / payment
        // flows never have to special-case a missing ledger.
        var ledger = new VendorLedger
        {
            Vendor = vendor,
            GrnTotal = 0,
            ReturnTotal = 0,
            PaidCredit = 0,
            OutstandingBalance = 0
        };

        await _unitOfWork.VendorLedgers.AddAsync(ledger, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vendor {VendorCode} created successfully with vendor id {VendorId}", vendor.VendorCode, vendor.VendorId);

        vendor.VendorLedger = ledger;
        return _mapper.Map<VendorDto>(vendor);
    }

    public async Task<VendorDto> UpdateAsync(int vendorId, UpdateVendorDto request, CancellationToken cancellationToken = default)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdWithLedgerAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException("Vendor", vendorId);

        vendor.VendorName = request.VendorName.Trim();
        vendor.Address = request.Address;
        vendor.Phone = request.Phone;
        vendor.Email = request.Email;
        vendor.ContactPerson = request.ContactPerson;
        vendor.IsActive = request.IsActive;

        _unitOfWork.Vendors.Update(vendor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vendor {VendorCode} updated successfully", vendor.VendorCode);

        return _mapper.Map<VendorDto>(vendor);
    }

    public async Task DeleteAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdWithLedgerAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException("Vendor", vendorId);

        if (await _unitOfWork.Vendors.HasPurchaseOrdersAsync(vendorId, cancellationToken))
        {
            throw new ConflictException($"Vendor '{vendor.VendorCode}' cannot be deleted while purchase orders reference it.");
        }

        if (await _unitOfWork.Vendors.HasGrnsAsync(vendorId, cancellationToken))
        {
            throw new ConflictException($"Vendor '{vendor.VendorCode}' cannot be deleted while GRNs reference it.");
        }

        if (vendor.VendorLedger is { OutstandingBalance: not 0 })
        {
            throw new ConflictException(
                $"Vendor '{vendor.VendorCode}' has an outstanding ledger balance of {vendor.VendorLedger.OutstandingBalance:N2} and cannot be deleted.");
        }

        // VendorLedgerConfiguration cascades on delete, so removing the vendor is enough.
        _unitOfWork.Vendors.Remove(vendor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vendor {VendorCode} deleted successfully", vendor.VendorCode);
    }
}
