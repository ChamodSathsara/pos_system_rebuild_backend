using AutoMapper;
using PosApi.DTOs.Product;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class TaxMasterService : ITaxMasterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TaxMasterService> _logger;

    public TaxMasterService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TaxMasterService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaxMasterDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var taxes = await _unitOfWork.TaxMasters.GetAllAsync(isActive, cancellationToken);
        return _mapper.Map<IReadOnlyList<TaxMasterDto>>(taxes);
    }

    public async Task<TaxMasterDto> GetByCodeAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        var tax = await _unitOfWork.TaxMasters.GetByIdAsync(taxCode, cancellationToken)
            ?? throw new NotFoundException("TaxMaster", taxCode);

        return _mapper.Map<TaxMasterDto>(tax);
    }

    public async Task<TaxMasterDto> CreateAsync(CreateTaxMasterDto request, CancellationToken cancellationToken = default)
    {
        var taxCode = request.TaxCode.Trim();

        if (await _unitOfWork.TaxMasters.TaxCodeExistsAsync(taxCode, cancellationToken))
        {
            throw new ConflictException($"A tax with code '{taxCode}' already exists.");
        }

        var tax = new TaxMaster
        {
            TaxCode = taxCode,
            TaxName = request.TaxName.Trim(),
            Percentage = request.Percentage,
            Description = request.Description,
            IsActive = request.IsActive
        };

        await _unitOfWork.TaxMasters.AddAsync(tax, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tax {TaxCode} created successfully", tax.TaxCode);

        return _mapper.Map<TaxMasterDto>(tax);
    }

    public async Task<TaxMasterDto> UpdateAsync(string taxCode, UpdateTaxMasterDto request, CancellationToken cancellationToken = default)
    {
        var tax = await _unitOfWork.TaxMasters.GetByIdAsync(taxCode, cancellationToken)
            ?? throw new NotFoundException("TaxMaster", taxCode);

        tax.TaxName = request.TaxName.Trim();
        tax.Percentage = request.Percentage;
        tax.Description = request.Description;
        tax.IsActive = request.IsActive;

        _unitOfWork.TaxMasters.Update(tax);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tax {TaxCode} updated successfully", taxCode);

        return _mapper.Map<TaxMasterDto>(tax);
    }

    public async Task DeleteAsync(string taxCode, CancellationToken cancellationToken = default)
    {
        var tax = await _unitOfWork.TaxMasters.GetByIdAsync(taxCode, cancellationToken)
            ?? throw new NotFoundException("TaxMaster", taxCode);

        if (await _unitOfWork.TaxMasters.HasProductsAsync(taxCode, cancellationToken))
        {
            throw new ConflictException($"Tax '{taxCode}' has products assigned to it and cannot be deleted.");
        }

        _unitOfWork.TaxMasters.Remove(tax);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tax {TaxCode} deleted successfully", taxCode);
    }
}
