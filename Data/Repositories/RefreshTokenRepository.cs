using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken, int>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(rt => rt.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
    }

    public Task<int> TryRevokeAsync(int id, string replacementTokenHash, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return DbSet
            .Where(rt => rt.Id == id && rt.RevokedAt == null && rt.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(rt => rt.RevokedAt, now)
                .SetProperty(rt => rt.ReplacedByTokenHash, replacementTokenHash), cancellationToken);
    }

    public async Task RevokeActiveFamilyAsync(string userCode, string familyId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await DbSet
            .Where(rt => rt.UserCode == userCode && rt.FamilyId == familyId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.RevokedAt, now), cancellationToken);
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
