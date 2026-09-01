using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class UserRepository : GenericRepository<SystemUser, string>, IUserRepository
{
    
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<SystemUser?> GetByUsernameWithRoleAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await DbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);


        return user;
    }

    public async Task<SystemUser?> GetByUserCodeWithRoleAsync(string userCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserCode == userCode, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<IReadOnlyList<SystemUser>> GetAllWithRoleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(u => u.Role)
            .ToListAsync(cancellationToken);
    }
}
