using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken, int>
{
    Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensForUserAsync(string userCode, CancellationToken cancellationToken = default);
    void RevokeToken(RefreshToken refreshToken);
}
