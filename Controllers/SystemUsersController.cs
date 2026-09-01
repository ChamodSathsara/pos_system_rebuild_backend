using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.Constants;
using PosApi.DTOs.Security;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// System user (staff account) management endpoints. Distinct from AuthController, which
/// handles login/logout/token-refresh for the currently authenticated user. Restricted to
/// Admins since this manages other users' accounts and access.
/// </summary>
[ApiController]
[Route("api/system-users")]
[Authorize(Roles = RoleConstants.Admin)]
public class SystemUsersController : BaseApiController
{
    private readonly ISystemUserService _systemUserService;

    public SystemUsersController(ISystemUserService systemUserService)
    {
        _systemUserService = systemUserService;
    }

    /// <summary>
    /// Retrieves every system user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SystemUserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _systemUserService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SystemUserDto>>.SuccessResponse(users));
    }

    /// <summary>
    /// Retrieves a single system user by code.
    /// </summary>
    [HttpGet("{userCode}")]
    [ProducesResponseType(typeof(ApiResponse<SystemUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string userCode, CancellationToken cancellationToken)
    {
        var user = await _systemUserService.GetByCodeAsync(userCode, cancellationToken);
        return Ok(ApiResponse<SystemUserDto>.SuccessResponse(user));
    }

    /// <summary>
    /// Creates a new system user (staff account). If userCode is omitted, one is generated
    /// automatically (e.g. USR00001). The supplied password is hashed before storage.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SystemUserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSystemUserDto request, CancellationToken cancellationToken)
    {
        var user = await _systemUserService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetByCode),
            new { userCode = user.UserCode },
            ApiResponse<SystemUserDto>.SuccessResponse(user, "System user created successfully."));
    }

    /// <summary>
    /// Updates an existing system user's profile fields. Password changes are not handled
    /// here - see the auth endpoints for password reset flows.
    /// </summary>
    [HttpPut("{userCode}")]
    [ProducesResponseType(typeof(ApiResponse<SystemUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string userCode, [FromBody] UpdateSystemUserDto request, CancellationToken cancellationToken)
    {
        var user = await _systemUserService.UpdateAsync(userCode, request, cancellationToken);
        return Ok(ApiResponse<SystemUserDto>.SuccessResponse(user, "System user updated successfully."));
    }

    /// <summary>
    /// Deletes a system user.
    /// </summary>
    [HttpDelete("{userCode}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string userCode, CancellationToken cancellationToken)
    {
        await _systemUserService.DeleteAsync(userCode, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("System user deleted successfully."));
    }
}
