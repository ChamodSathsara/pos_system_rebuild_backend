using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IBranchRepository : IGenericRepository<Branch, string>
{
    Task<bool> BranchCodeExistsAsync(string branchCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Branch>> GetByCompanyCodeAsync(string companyCode, CancellationToken cancellationToken = default);
}
