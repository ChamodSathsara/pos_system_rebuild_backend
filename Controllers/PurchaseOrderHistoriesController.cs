using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Purchase;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Purchase order audit trail endpoints. Created/Modified/Cancelled/StatusChanged entries are
/// generated automatically by PurchaseOrdersController actions; this controller lets callers
/// browse that trail and record manual Approved/Rejected review notes.
/// </summary>
[ApiController]
[Route("api/purchase-order-histories")]
[Authorize]
public class PurchaseOrderHistoriesController : BaseApiController
{
    private readonly IPurchaseOrderHistoryService _purchaseOrderHistoryService;

    public PurchaseOrderHistoriesController(IPurchaseOrderHistoryService purchaseOrderHistoryService)
    {
        _purchaseOrderHistoryService = purchaseOrderHistoryService;
    }

    /// <summary>Retrieves the full history trail for a purchase order, most recent first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PurchaseOrderHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPoNo([FromQuery] string poNo, CancellationToken cancellationToken)
    {
        var histories = await _purchaseOrderHistoryService.GetByPoNoAsync(poNo, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PurchaseOrderHistoryDto>>.SuccessResponse(histories));
    }

    /// <summary>Retrieves a single history entry, including its field-level changes.</summary>
    [HttpGet("{historyId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int historyId, CancellationToken cancellationToken)
    {
        var history = await _purchaseOrderHistoryService.GetByIdAsync(historyId, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderHistoryDto>.SuccessResponse(history));
    }

    /// <summary>Manually records an Approved/Rejected review note. Other lifecycle actions are generated automatically.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderHistoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderHistoryDto request, CancellationToken cancellationToken)
    {
        var history = await _purchaseOrderHistoryService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { historyId = history.HistoryId },
            ApiResponse<PurchaseOrderHistoryDto>.SuccessResponse(history, "History entry recorded successfully."));
    }

    /// <summary>Corrects a history entry's Remarks. Action, ChangedBy, and ChangedAt are immutable audit facts.</summary>
    [HttpPut("{historyId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int historyId, [FromBody] UpdatePurchaseOrderHistoryDto request, CancellationToken cancellationToken)
    {
        var history = await _purchaseOrderHistoryService.UpdateAsync(historyId, request, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderHistoryDto>.SuccessResponse(history, "History entry updated successfully."));
    }

    /// <summary>Deletes a manual review note. System-generated lifecycle entries cannot be deleted.</summary>
    [HttpDelete("{historyId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int historyId, CancellationToken cancellationToken)
    {
        await _purchaseOrderHistoryService.DeleteAsync(historyId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("History entry deleted successfully."));
    }
}