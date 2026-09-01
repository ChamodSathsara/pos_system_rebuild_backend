using PosApi.DTOs.Security;

namespace PosApi.Service.Interfaces;

public interface IUserRoleService
{
    Task<IReadOnlyList<UserRoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserRoleDto> GetByIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<UserRoleWithPermissionsDto> GetByIdWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<UserRoleDto> CreateAsync(CreateUserRoleDto request, CancellationToken cancellationToken = default);
    Task<UserRoleDto> UpdateAsync(int roleId, UpdateUserRoleDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int roleId, CancellationToken cancellationToken = default);
}
