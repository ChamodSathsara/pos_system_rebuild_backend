using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface ICompanyRepository : IGenericRepository<Company, string>
{
    Task<bool> CompanyCodeExistsAsync(string companyCode, CancellationToken cancellationToken = default);
}
