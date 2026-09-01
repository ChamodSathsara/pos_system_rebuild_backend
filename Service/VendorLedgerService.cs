using AutoMapper;
using PosApi.DTOs.Vendor;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class VendorLedgerService : IVendorLedgerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<VendorLedgerService> _logger;

    public VendorLedgerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VendorLedgerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VendorLedgerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var ledgers = await _unitOfWork.VendorLedgers.GetAllWithVendorAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<VendorLedgerDto>>(ledgers);
    }

    public async Task<VendorLedgerDto> GetByIdAsync(int ledgerId, CancellationToken cancellationToken = default)
    {
        var ledger = await _unitOfWork.VendorLedgers.GetByIdWithVendorAsync(ledgerId, cancellationToken)
            ?? throw new NotFoundException("VendorLedger", ledgerId);

        return _mapper.Map<VendorLedgerDto>(ledger);
    }

    public async Task<VendorLedgerDto> GetByVendorIdAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var ledger = await _unitOfWork.VendorLedgers.GetByVendorIdAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException($"No ledger was found for vendor id '{vendorId}'.");

        return await GetByIdAsync(ledger.LedgerId, cancellationToken);
    }

    public async Task<VendorLedgerDto> CreateAsync(CreateVendorLedgerDto request, CancellationToken cancellationToken = default)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.VendorId, cancellationToken)
            ?? throw new NotFoundException("Vendor", request.VendorId);

        var existing = await _unitOfWork.VendorLedgers.GetByVendorIdAsync(request.VendorId, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"Vendor '{vendor.VendorCode}' already has a ledger (id {existing.LedgerId}).");
        }

        var ledger = new VendorLedger
        {
            VendorId = request.VendorId,
            GrnTotal = request.GrnTotal,
            ReturnTotal = request.ReturnTotal,
            PaidCredit = request.PaidCredit,
            OutstandingBalance = request.GrnTotal - request.ReturnTotal - request.PaidCredit
        };

        await _unitOfWork.VendorLedgers.AddAsync(ledger, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vendor ledger {LedgerId} created for vendor {VendorId}", ledger.LedgerId, ledger.VendorId);

        return await GetByIdAsync(ledger.LedgerId, cancellationToken);
    }

    public async Task<VendorLedgerDto> UpdateAsync(int ledgerId, UpdateVendorLedgerDto request, CancellationToken cancellationToken = default)
    {
        var ledger = await _unitOfWork.VendorLedgers.GetByIdAsync(ledgerId, cancellationToken)
            ?? throw new NotFoundException("VendorLedger", ledgerId);

        ledger.GrnTotal = request.GrnTotal;
        ledger.ReturnTotal = request.ReturnTotal;
        ledger.PaidCredit = request.PaidCredit;
        ledger.OutstandingBalance = request.GrnTotal - request.ReturnTotal - request.PaidCredit;

        _unitOfWork.VendorLedgers.Update(ledger);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vendor ledger {LedgerId} updated (manual correction)", ledgerId);

        return await GetByIdAsync(ledgerId, cancellationToken);
    }

    public async Task<VendorLedgerDto> RecordPaymentAsync(int vendorId, RecordVendorPaymentDto request, CancellationToken cancellationToken = default)
    {
        var ledger = await _unitOfWork.VendorLedgers.GetByVendorIdAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException($"No ledger was found for vendor id '{vendorId}'.");

        ledger.PaidCredit = (ledger.PaidCredit ?? 0) + request.Amount;
        ledger.OutstandingBalance = (ledger.GrnTotal ?? 0) - (ledger.ReturnTotal ?? 0) - (ledger.PaidCredit ?? 0);

        _unitOfWork.VendorLedgers.Update(ledger);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Recorded payment of {Amount:N2} for vendor {VendorId}; new outstanding balance {Balance:N2}",
            request.Amount, vendorId, ledger.OutstandingBalance);

        return await GetByIdAsync(ledger.LedgerId, cancellationToken);
    }

    public async Task DeleteAsync(int ledgerId, CancellationToken cancellationToken = default)
    {
        var ledger = await _unitOfWork.VendorLedgers.GetByIdAsync(ledgerId, cancellationToken)
            ?? throw new NotFoundException("VendorLedger", ledgerId);

        if (ledger.VendorId is not null)
        {
            throw new ConflictException(
                "This ledger is linked to a vendor and cannot be deleted directly. Delete the vendor instead, or move the balance to zero first.");
        }

        _unitOfWork.VendorLedgers.Remove(ledger);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vendor ledger {LedgerId} deleted successfully", ledgerId);
    }
}
