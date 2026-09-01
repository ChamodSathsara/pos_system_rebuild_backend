using PosApi.Models.Entities;

namespace PosApi.Security;

public interface IJwtTokenGenerator
{
    /// <summary>
    /// Builds a signed JWT access token for the given user, embedding user_id, role and
    /// username/email claims.
    /// </summary>
    (string Token, DateTime ExpiresAt) GenerateAccessToken(SystemUser user);

    /// <summary>
    /// Generates a cryptographically random opaque refresh token string (not a JWT).
    /// </summary>
    string GenerateRefreshToken();
}
