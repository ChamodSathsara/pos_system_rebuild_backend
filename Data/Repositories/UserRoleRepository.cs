using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class UserRoleRepository : GenericRepository<UserRole, int>, IUserRoleRepository
{
    public UserRoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> RoleNameExistsAsync(string roleName, int? excludeRoleId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .AnyAsync(r => r.RoleName == roleName && (excludeRoleId == null || r.RoleId != excludeRoleId), cancellationToken);
    }

    public async Task<UserRole?> GetByIdWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.UserRolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);
    }
}
