using AutoMapper;
using PosApi.DTOs.Organization;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CompanyService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CompanyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _unitOfWork.Companies.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CompanyDto>>(companies);
    }

    public async Task<CompanyDto> GetByCodeAsync(string companyCode, CancellationToken cancellationToken = default)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyCode, cancellationToken)
            ?? throw new NotFoundException("Company", companyCode);

        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto request, CancellationToken cancellationToken = default)
    {
        var companyCode = request.CompanyCode.Trim();

        if (await _unitOfWork.Companies.CompanyCodeExistsAsync(companyCode, cancellationToken))
        {
            throw new ConflictException($"A company with code '{companyCode}' already exists.");
        }

        var company = new Company
        {
            CompanyCode = companyCode,
            CompanyName = request.CompanyName.Trim(),
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            RegistrationNo = request.RegistrationNo,
            TaxId = request.TaxId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Companies.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Company {CompanyCode} created successfully", company.CompanyCode);

        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CompanyDto> UpdateAsync(string companyCode, UpdateCompanyDto request, CancellationToken cancellationToken = default)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyCode, cancellationToken)
            ?? throw new NotFoundException("Company", companyCode);

        company.CompanyName = request.CompanyName.Trim();
        company.Address = request.Address;
        company.Phone = request.Phone;
        company.Email = request.Email;
        company.RegistrationNo = request.RegistrationNo;
        company.TaxId = request.TaxId;
        company.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Company {CompanyCode} updated successfully", company.CompanyCode);

        return _mapper.Map<CompanyDto>(company);
    }

    public async Task DeleteAsync(string companyCode, CancellationToken cancellationToken = default)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyCode, cancellationToken)
            ?? throw new NotFoundException("Company", companyCode);

        var hasBranches = await _unitOfWork.Branches.ExistsAsync(b => b.CompanyCode == companyCode, cancellationToken);
        if (hasBranches)
        {
            throw new ConflictException($"Company '{companyCode}' cannot be deleted while it still has branches assigned to it.");
        }

        _unitOfWork.Companies.Remove(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Company {CompanyCode} deleted successfully", companyCode);
    }
}
