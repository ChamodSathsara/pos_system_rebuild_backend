using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Grn;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// GRN return endpoints. Posting a return sends previously received stock back to the vendor in
/// one transaction: it draws down stock inventory/batches, raises a stock movement, rolls back the
/// source purchase order (items, status, history), and posts the total to the vendor's ledger.
/// </summary>
[ApiController]
[Route("api/grn-returns")]
[Authorize]
public class GrnReturnsController : BaseApiController
{
    private readonly IGrnReturnService _grnReturnService;

    public GrnReturnsController(IGrnReturnService grnReturnService)
    {
        _grnReturnService = grnReturnService;
    }

    /// <summary>Searches GRN returns, optionally filtered by GRN, status, or return date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GrnReturnDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] int? grnId,
        [FromQuery] GrnReturnStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var returns = await _grnReturnService.SearchAsync(grnId, status, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GrnReturnDto>>.SuccessResponse(returns));
    }

    /// <summary>Retrieves a single GRN return, including its line items.</summary>
    [HttpGet("{grnReturnId:int}")]
    [ProducesResponseType(typeof(ApiResponse<GrnReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int grnReturnId, CancellationToken cancellationToken)
    {
        var grnReturn = await _grnReturnService.GetByIdAsync(grnReturnId, cancellationToken);
        return Ok(ApiResponse<GrnReturnDto>.SuccessResponse(grnReturn));
    }

    /// <summary>
    /// Posts a return against a GRN. Drives the full return flow in one transaction: inserts the
    /// return header/lines, draws down stock inventory and the matching batch, raises a stock-out
    /// movement, rolls back the source purchase order's items/status/history, and posts the total
    /// to the vendor's ledger.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GrnReturnDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateGrnReturnDto request, CancellationToken cancellationToken)
    {
        var grnReturn = await _grnReturnService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { grnReturnId = grnReturn.GrnReturnId },
            ApiResponse<GrnReturnDto>.SuccessResponse(grnReturn, "GRN return posted successfully."));
    }
}
