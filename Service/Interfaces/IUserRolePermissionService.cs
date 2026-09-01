using PosApi.DTOs.Security;

namespace PosApi.Service.Interfaces;

public interface IUserRolePermissionService
{
    Task<IReadOnlyList<UserRolePermissionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserRolePermissionDto>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<UserRolePermissionDto> AssignAsync(int roleId, AssignPermissionDto request, CancellationToken cancellationToken = default);
    Task RemoveAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);
}
