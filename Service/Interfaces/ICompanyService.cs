using PosApi.DTOs.Organization;

namespace PosApi.Service.Interfaces;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CompanyDto> GetByCodeAsync(string companyCode, CancellationToken cancellationToken = default);
    Task<CompanyDto> CreateAsync(CreateCompanyDto request, CancellationToken cancellationToken = default);
    Task<CompanyDto> UpdateAsync(string companyCode, UpdateCompanyDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string companyCode, CancellationToken cancellationToken = default);
}
