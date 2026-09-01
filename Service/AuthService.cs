using Microsoft.Extensions.Options;
using PosApi.Configuration;
using PosApi.DTOs.Auth;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Security;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByUsernameWithRoleAsync(request.Username, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username {Username}", request.Username);
            throw new UnauthorizedAppException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException("This account has been deactivated. Please contact an administrator.");
        }

        var (accessToken, accessTokenExpiresAt) = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        var refreshToken = new RefreshToken
        {
            UserCode = user.UserCode,
            Token = refreshTokenValue,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        user.LastLogin = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = MapToCurrentUserDto(user)
        };
    }

    public async Task LogoutAsync(string userCode, string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var token = await _unitOfWork.RefreshTokens.GetActiveTokenAsync(refreshToken, cancellationToken);
            if (token is not null && token.UserCode == userCode)
            {
                _unitOfWork.RefreshTokens.RevokeToken(token);
            }
        }
        else
        {
            var activeTokens = await _unitOfWork.RefreshTokens.GetActiveTokensForUserAsync(userCode, cancellationToken);
            foreach (var token in activeTokens)
            {
                _unitOfWork.RefreshTokens.RevokeToken(token);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(string userCode, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByUserCodeWithRoleAsync(userCode, cancellationToken)
            ?? throw new NotFoundException("User", userCode);

        return MapToCurrentUserDto(user);
    }

    private static CurrentUserDto MapToCurrentUserDto(SystemUser user)
    {
        return new CurrentUserDto
        {
            UserCode = user.UserCode,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Mobile = user.Mobile,
            BranchCode = user.BranchCode,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName,
            IsActive = user.IsActive,
            LastLogin = user.LastLogin
        };
    }
}
