using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class PermissionRepository : GenericRepository<Permission, int>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> PermissionNameExistsAsync(string permissionName, int? excludePermissionId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .AnyAsync(p => p.PermissionName == permissionName && (excludePermissionId == null || p.PermissionId != excludePermissionId), cancellationToken);
    }
}
