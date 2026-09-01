using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Grn;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// GRN (Goods Received Note) endpoints. Posting a GRN receives stock against a purchase order in
/// one transaction: it updates stock inventory/batches, raises a stock movement, updates the
/// purchase order (items, status, history), and posts the total to the vendor's ledger.
/// </summary>
[ApiController]
[Route("api/grn-masters")]
[Authorize]
public class GrnMastersController : BaseApiController
{
    private readonly IGrnMasterService _grnMasterService;

    public GrnMastersController(IGrnMasterService grnMasterService)
    {
        _grnMasterService = grnMasterService;
    }

    /// <summary>Searches GRNs, optionally filtered by PO, vendor, branch, warehouse, or GRN date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GrnDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? poNo,
        [FromQuery] int? vendorId,
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var grns = await _grnMasterService.SearchAsync(poNo, vendorId, branchCode, warehouseCode, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GrnDto>>.SuccessResponse(grns));
    }

    /// <summary>Retrieves a single GRN, including its line items.</summary>
    [HttpGet("{grnId:int}")]
    [ProducesResponseType(typeof(ApiResponse<GrnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int grnId, CancellationToken cancellationToken)
    {
        var grn = await _grnMasterService.GetByIdAsync(grnId, cancellationToken);
        return Ok(ApiResponse<GrnDto>.SuccessResponse(grn));
    }

    /// <summary>
    /// Posts a GRN against an open/partially-received purchase order. Drives the full receiving
    /// flow in one transaction: inserts the GRN header/lines, tops up stock inventory, opens a new
    /// batch per line, raises a stock-in movement, updates the source purchase order's items/status/
    /// history, and posts the total to the vendor's ledger.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GrnDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateGrnDto request, CancellationToken cancellationToken)
    {
        var grn = await _grnMasterService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { grnId = grn.GrnId },
            ApiResponse<GrnDto>.SuccessResponse(grn, "GRN posted successfully."));
    }

    /// <summary>Deletes a GRN and reverses every downstream effect. Only allowed while nothing has been returned against it and its batches remain untouched.</summary>
    [HttpDelete("{grnId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int grnId, CancellationToken cancellationToken)
    {
        await _grnMasterService.DeleteAsync(grnId, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("GRN deleted successfully."));
    }
}
