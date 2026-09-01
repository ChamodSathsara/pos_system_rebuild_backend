using PosApi.DTOs.Security;

namespace PosApi.Service.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PermissionDto> GetByIdAsync(int permissionId, CancellationToken cancellationToken = default);
    Task<PermissionDto> CreateAsync(CreatePermissionDto request, CancellationToken cancellationToken = default);
    Task<PermissionDto> UpdateAsync(int permissionId, UpdatePermissionDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int permissionId, CancellationToken cancellationToken = default);
}
