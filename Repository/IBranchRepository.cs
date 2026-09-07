using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IBranchRepository : IGenericRepository<Branch, string>
{
    Task<bool> BranchCodeExistsAsync(string branchCode, CancellationToken cancellationToken = default);

    Task<string> GenerateNextBranchCodeAsync(CancellationToken cancellationToken = default);

    Task<bool> BranchNameExistsAsync(
        string branchName,
        string? excludeBranchCode = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Branch>> GetByCompanyCodeAsync(string companyCode, CancellationToken cancellationToken = default);
}
