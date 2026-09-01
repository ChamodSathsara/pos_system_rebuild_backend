using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Grn;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// GRN return line item endpoints. Read-only: lines are only ever created as part of posting a
/// GRN return via GrnReturnsController, since each one drives stock/batch/ledger side effects.
/// </summary>
[ApiController]
[Route("api/grn-return-items")]
[Authorize]
public class GrnReturnItemsController : BaseApiController
{
    private readonly IGrnReturnItemService _grnReturnItemService;

    public GrnReturnItemsController(IGrnReturnItemService grnReturnItemService)
    {
        _grnReturnItemService = grnReturnItemService;
    }

    /// <summary>Retrieves all line items for a GRN return.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GrnReturnItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGrnReturnId([FromQuery] int grnReturnId, CancellationToken cancellationToken)
    {
        var items = await _grnReturnItemService.GetByGrnReturnIdAsync(grnReturnId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GrnReturnItemDto>>.SuccessResponse(items));
    }

    /// <summary>Retrieves a single GRN return line item by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GrnReturnItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _grnReturnItemService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<GrnReturnItemDto>.SuccessResponse(item));
    }
}
