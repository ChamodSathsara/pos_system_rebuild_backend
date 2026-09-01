using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.Constants;
using PosApi.DTOs.Security;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Permission management endpoints. Restricted to Admins.
/// </summary>
[ApiController]
[Route("api/permissions")]
[Authorize(Roles = RoleConstants.Admin)]
public class PermissionsController : BaseApiController
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Retrieves every permission.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PermissionDto>>.SuccessResponse(permissions));
    }

    /// <summary>
    /// Retrieves a single permission by id.
    /// </summary>
    [HttpGet("{permissionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int permissionId, CancellationToken cancellationToken)
    {
        var permission = await _permissionService.GetByIdAsync(permissionId, cancellationToken);
        return Ok(ApiResponse<PermissionDto>.SuccessResponse(permission));
    }

    /// <summary>
    /// Creates a new permission.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePermissionDto request, CancellationToken cancellationToken)
    {
        var permission = await _permissionService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { permissionId = permission.PermissionId },
            ApiResponse<PermissionDto>.SuccessResponse(permission, "Permission created successfully."));
    }

    /// <summary>
    /// Updates an existing permission.
    /// </summary>
    [HttpPut("{permissionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int permissionId, [FromBody] UpdatePermissionDto request, CancellationToken cancellationToken)
    {
        var permission = await _permissionService.UpdateAsync(permissionId, request, cancellationToken);
        return Ok(ApiResponse<PermissionDto>.SuccessResponse(permission, "Permission updated successfully."));
    }

    /// <summary>
    /// Deletes a permission. Fails if it is still assigned to any role.
    /// </summary>
    [HttpDelete("{permissionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int permissionId, CancellationToken cancellationToken)
    {
        await _permissionService.DeleteAsync(permissionId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Permission deleted successfully."));
    }
}
