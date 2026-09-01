using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Purchase;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Field-level change records attached to a PurchaseOrderHistory entry. Most are generated
/// automatically by PurchaseOrdersController/PurchaseOrderItemsController actions; this
/// controller exists for browsing them and for manual corrections.
/// </summary>
[ApiController]
[Route("api/purchase-order-history-changes")]
[Authorize]
public class PurchaseOrderHistoryChangesController : BaseApiController
{
    private readonly IPurchaseOrderHistoryChangeService _purchaseOrderHistoryChangeService;

    public PurchaseOrderHistoryChangesController(IPurchaseOrderHistoryChangeService purchaseOrderHistoryChangeService)
    {
        _purchaseOrderHistoryChangeService = purchaseOrderHistoryChangeService;
    }

    /// <summary>Retrieves all change records for a history entry.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PurchaseOrderHistoryChangeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByHistoryId([FromQuery] int historyId, CancellationToken cancellationToken)
    {
        var changes = await _purchaseOrderHistoryChangeService.GetByHistoryIdAsync(historyId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PurchaseOrderHistoryChangeDto>>.SuccessResponse(changes));
    }

    /// <summary>Retrieves a single change record by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderHistoryChangeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var change = await _purchaseOrderHistoryChangeService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderHistoryChangeDto>.SuccessResponse(change));
    }

    /// <summary>Creates a change record under an existing history entry.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderHistoryChangeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderHistoryChangeDto request, CancellationToken cancellationToken)
    {
        var change = await _purchaseOrderHistoryChangeService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = change.Id },
            ApiResponse<PurchaseOrderHistoryChangeDto>.SuccessResponse(change, "History change recorded successfully."));
    }

    /// <summary>Corrects a change record's field/old/new values.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderHistoryChangeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderHistoryChangeDto request, CancellationToken cancellationToken)
    {
        var change = await _purchaseOrderHistoryChangeService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderHistoryChangeDto>.SuccessResponse(change, "History change updated successfully."));
    }

    /// <summary>Deletes a change record.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _purchaseOrderHistoryChangeService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("History change deleted successfully."));
    }
}