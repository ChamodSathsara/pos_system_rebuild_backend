using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken, int>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensForUserAsync(string userCode, CancellationToken cancellationToken = default);
    Task<int> TryRevokeAsync(int id, string replacementTokenHash, CancellationToken cancellationToken = default);
    Task RevokeActiveFamilyAsync(string userCode, string familyId, CancellationToken cancellationToken = default);
    void RevokeToken(RefreshToken refreshToken);
}
