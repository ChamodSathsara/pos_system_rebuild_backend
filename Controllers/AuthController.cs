using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Auth;
using PosApi.Extensions;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Authentication endpoints: login, logout, current-user lookup, and a protected smoke-test route.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a system_user by username/password and issues a JWT access token plus a
    /// refresh token. The access token carries user_id, role and username/email claims.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result, "Login successful."));
    }

    /// <summary>
    /// Revokes the caller's refresh token(s). Requires a valid access token. If a specific
    /// refresh token is supplied in the body only that token is revoked, otherwise every active
    /// refresh token for the user is revoked (logout from all devices).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto? request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(CurrentUserCode, request?.RefreshToken, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Logout successful."));
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var user = await _authService.GetCurrentUserAsync(CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<CurrentUserDto>.SuccessResponse(user));
    }

    /// <summary>
    /// Simple protected endpoint for verifying that a bearer token is valid and correctly
    /// authenticated end-to-end (useful for smoke-testing client integrations).
    /// </summary>
    [HttpGet("test")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Test()
    {
        var payload = new
        {
            Message = "You are authenticated.",
            UserCode = User.GetUserCode(),
            Username = User.GetUsername(),
            Role = User.GetRole()
        };

        return Ok(ApiResponse<object>.SuccessResponse(payload, "JWT authentication is working."));
    }


    [HttpGet("hashpassword")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public String Test2()
    {
        var password = "00000000";

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        Console.WriteLine(passwordHash);
        return passwordHash;
    }

    
}
