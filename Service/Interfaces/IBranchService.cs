using PosApi.DTOs.Organization;

namespace PosApi.Service.Interfaces;

public interface IBranchService
{
    Task<IReadOnlyList<BranchDto>> GetAllAsync(string? companyCode = null, CancellationToken cancellationToken = default);
    Task<BranchDto> GetByCodeAsync(string branchCode, CancellationToken cancellationToken = default);
    Task<BranchDto> CreateAsync(CreateBranchDto request, CancellationToken cancellationToken = default);
    Task<BranchDto> UpdateAsync(string branchCode, UpdateBranchDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string branchCode, CancellationToken cancellationToken = default);
}
