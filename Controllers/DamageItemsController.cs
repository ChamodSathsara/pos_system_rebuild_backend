using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Stock;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Damaged/written-off item report endpoints. Reports are recorded against an item and branch
/// (and optionally a warehouse) and move through a review workflow via Status
/// (Reported -> Reviewed -> Approved -> Disposed, or Rejected). ReportedBy is always set to the
/// currently authenticated user recording the report.
/// </summary>
[ApiController]
[Route("api/damage-items")]
[Authorize]
public class DamageItemsController : BaseApiController
{
    private readonly IDamageItemService _damageItemService;

    public DamageItemsController(IDamageItemService damageItemService)
    {
        _damageItemService = damageItemService;
    }

    /// <summary>Searches damage reports, optionally filtered by item, branch, warehouse, status, or damage date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DamageItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? itemCode,
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] DamageItemStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var damageItems = await _damageItemService.SearchAsync(itemCode, branchCode, warehouseCode, status, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DamageItemDto>>.SuccessResponse(damageItems));
    }

    /// <summary>Retrieves a single damage report.</summary>
    [HttpGet("{damageId:int}")]
    [ProducesResponseType(typeof(ApiResponse<DamageItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int damageId, CancellationToken cancellationToken)
    {
        var damageItem = await _damageItemService.GetByIdAsync(damageId, cancellationToken);
        return Ok(ApiResponse<DamageItemDto>.SuccessResponse(damageItem));
    }

    /// <summary>Records a new damage report. ReportedBy is set to the currently authenticated user. Status always starts at Reported.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DamageItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateDamageItemDto request, CancellationToken cancellationToken)
    {
        var damageItem = await _damageItemService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { damageId = damageItem.DamageId },
            ApiResponse<DamageItemDto>.SuccessResponse(damageItem, "Damage report recorded successfully."));
    }

    /// <summary>Updates a damage report's details and/or advances its review Status. ReportedBy is immutable.</summary>
    [HttpPut("{damageId:int}")]
    [ProducesResponseType(typeof(ApiResponse<DamageItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int damageId, [FromBody] UpdateDamageItemDto request, CancellationToken cancellationToken)
    {
        var damageItem = await _damageItemService.UpdateAsync(damageId, request, cancellationToken);
        return Ok(ApiResponse<DamageItemDto>.SuccessResponse(damageItem, "Damage report updated successfully."));
    }

    /// <summary>Deletes a damage report.</summary>
    [HttpDelete("{damageId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int damageId, CancellationToken cancellationToken)
    {
        await _damageItemService.DeleteAsync(damageId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Damage report deleted successfully."));
    }
}
