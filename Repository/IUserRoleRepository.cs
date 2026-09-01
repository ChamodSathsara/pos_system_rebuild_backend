using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IUserRoleRepository : IGenericRepository<UserRole, int>
{
    Task<bool> RoleNameExistsAsync(string roleName, int? excludeRoleId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a role together with its assigned permissions.
    /// </summary>
    Task<UserRole?> GetByIdWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default);
}
