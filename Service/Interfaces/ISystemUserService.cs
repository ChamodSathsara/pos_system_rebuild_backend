using PosApi.DTOs.Security;

namespace PosApi.Service.Interfaces;

public interface ISystemUserService
{
    Task<IReadOnlyList<SystemUserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SystemUserDto> GetByCodeAsync(string userCode, CancellationToken cancellationToken = default);
    Task<SystemUserDto> CreateAsync(CreateSystemUserDto request, CancellationToken cancellationToken = default);
    Task<SystemUserDto> UpdateAsync(string userCode, UpdateSystemUserDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string userCode, CancellationToken cancellationToken = default);
}
