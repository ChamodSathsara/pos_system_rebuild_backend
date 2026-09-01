using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class UserRolePermissionRepository : IUserRolePermissionRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<UserRolePermission> _dbSet;

    public UserRolePermissionRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<UserRolePermission>();
    }

    public async Task<IReadOnlyList<UserRolePermission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserRolePermission>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserRolePermission?> GetAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task<bool> ExistsForPermissionAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(rp => rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task AddAsync(UserRolePermission entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Remove(UserRolePermission entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
