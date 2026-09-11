using PosApi.DTOs.Security;

namespace PosApi.Service.Interfaces;

public interface ISystemUserService
{
    Task<IReadOnlyList<SystemUserDto>> GetAllAsync(string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default);
    Task<SystemUserDto> GetByCodeAsync(string userCode, string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default);
    Task<SystemUserDto> CreateAsync(CreateSystemUserDto request, string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default);
    Task<SystemUserDto> UpdateAsync(string userCode, UpdateSystemUserDto request, string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default);
    Task DeleteAsync(string userCode, string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default);
}
