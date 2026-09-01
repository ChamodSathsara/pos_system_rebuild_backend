using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken, int>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(rt => rt.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.RevokedAt == null && rt.ExpiresAt > now, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensForUserAsync(string userCode, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(rt => rt.UserCode == userCode && rt.RevokedAt == null && rt.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }

    public void RevokeToken(RefreshToken refreshToken)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        DbSet.Update(refreshToken);
    }
}
