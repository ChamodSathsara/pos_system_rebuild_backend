using PosApi.DTOs.Auth;

namespace PosApi.Service.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task LogoutAsync(string userCode, string? refreshToken, CancellationToken cancellationToken = default);
    Task<CurrentUserDto> GetCurrentUserAsync(string userCode, CancellationToken cancellationToken = default);
}
