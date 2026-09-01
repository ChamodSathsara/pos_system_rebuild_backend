using PosApi.Models.Entities;

namespace PosApi.Repository;

/// <summary>
/// Data-access contract for the user_role_permission join table. Not modeled through
/// IGenericRepository because the entity has a composite key (RoleId + PermissionId)
/// rather than a single TKey.
/// </summary>
public interface IUserRolePermissionRepository
{
    Task<IReadOnlyList<UserRolePermission>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserRolePermission>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);

    Task<UserRolePermission?> GetAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForPermissionAsync(int permissionId, CancellationToken cancellationToken = default);

    Task AddAsync(UserRolePermission entity, CancellationToken cancellationToken = default);

    void Remove(UserRolePermission entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
