using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Product;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Item change-log endpoints - an audit trail of changes made against a product/item. ChangedBy
/// is always set to the currently authenticated user recording the entry; entries have no
/// editable descriptive fields, so only Search, GetById, Create, and Delete are exposed.
/// </summary>
[ApiController]
[Route("api/item-logs")]
[Authorize]
public class ItemLogsController : BaseApiController
{
    private readonly IItemLogService _itemLogService;

    public ItemLogsController(IItemLogService itemLogService)
    {
        _itemLogService = itemLogService;
    }

    /// <summary>Searches item logs, optionally filtered by item, action, changed-by user, or change date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ItemLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? itemCode,
        [FromQuery] string? action,
        [FromQuery] string? changedBy,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var itemLogs = await _itemLogService.SearchAsync(itemCode, action, changedBy, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ItemLogDto>>.SuccessResponse(itemLogs));
    }

    /// <summary>Retrieves a single item log entry.</summary>
    [HttpGet("{logId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int logId, CancellationToken cancellationToken)
    {
        var itemLog = await _itemLogService.GetByIdAsync(logId, cancellationToken);
        return Ok(ApiResponse<ItemLogDto>.SuccessResponse(itemLog));
    }

    /// <summary>Records a new item change-log entry. ChangedBy is set to the currently authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ItemLogDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateItemLogDto request, CancellationToken cancellationToken)
    {
        var itemLog = await _itemLogService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { logId = itemLog.LogId },
            ApiResponse<ItemLogDto>.SuccessResponse(itemLog, "Item log recorded successfully."));
    }

    /// <summary>Deletes an item log entry.</summary>
    [HttpDelete("{logId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int logId, CancellationToken cancellationToken)
    {
        await _itemLogService.DeleteAsync(logId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Item log deleted successfully."));
    }
}
