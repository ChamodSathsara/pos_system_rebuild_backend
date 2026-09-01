using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Organization;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Branch management endpoints.
/// </summary>
[ApiController]
[Route("api/branches")]
[Authorize]
public class BranchesController : BaseApiController
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    /// <summary>
    /// Retrieves branches, optionally filtered by company code.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BranchDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? companyCode, CancellationToken cancellationToken)
    {
        var branches = await _branchService.GetAllAsync(companyCode, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BranchDto>>.SuccessResponse(branches));
    }

    /// <summary>
    /// Retrieves a single branch by code.
    /// </summary>
    [HttpGet("{branchCode}")]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string branchCode, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetByCodeAsync(branchCode, cancellationToken);
        return Ok(ApiResponse<BranchDto>.SuccessResponse(branch));
    }

    /// <summary>
    /// Creates a new branch.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto request, CancellationToken cancellationToken)
    {
        var branch = await _branchService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetByCode),
            new { branchCode = branch.BranchCode },
            ApiResponse<BranchDto>.SuccessResponse(branch, "Branch created successfully."));
    }

    /// <summary>
    /// Updates an existing branch.
    /// </summary>
    [HttpPut("{branchCode}")]
    [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string branchCode, [FromBody] UpdateBranchDto request, CancellationToken cancellationToken)
    {
        var branch = await _branchService.UpdateAsync(branchCode, request, cancellationToken);
        return Ok(ApiResponse<BranchDto>.SuccessResponse(branch, "Branch updated successfully."));
    }

    /// <summary>
    /// Deletes a branch. Fails if it still has warehouses or system users assigned to it.
    /// </summary>
    [HttpDelete("{branchCode}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string branchCode, CancellationToken cancellationToken)
    {
        await _branchService.DeleteAsync(branchCode, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Branch deleted successfully."));
    }
}
