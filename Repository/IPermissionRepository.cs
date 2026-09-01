using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IPermissionRepository : IGenericRepository<Permission, int>
{
    Task<bool> PermissionNameExistsAsync(string permissionName, int? excludePermissionId = null, CancellationToken cancellationToken = default);
}
