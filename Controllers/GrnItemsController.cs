using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Grn;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// GRN line item endpoints. Read-only: lines are only ever created as part of posting a GRN via
/// GrnMastersController, since each one drives stock/batch/ledger side effects.
/// </summary>
[ApiController]
[Route("api/grn-items")]
[Authorize]
public class GrnItemsController : BaseApiController
{
    private readonly IGrnItemService _grnItemService;

    public GrnItemsController(IGrnItemService grnItemService)
    {
        _grnItemService = grnItemService;
    }

    /// <summary>Retrieves all line items for a GRN.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GrnItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGrnId([FromQuery] int grnId, CancellationToken cancellationToken)
    {
        var items = await _grnItemService.GetByGrnIdAsync(grnId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GrnItemDto>>.SuccessResponse(items));
    }

    /// <summary>Retrieves a single GRN line item by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GrnItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _grnItemService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<GrnItemDto>.SuccessResponse(item));
    }
}
