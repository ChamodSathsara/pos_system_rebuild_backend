using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Purchase;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Purchase order line item endpoints, for surgical single-line edits to an open order.
/// Prefer creating a full order via PurchaseOrdersController when possible.
/// </summary>
[ApiController]
[Route("api/purchase-order-items")]
[Authorize]
public class PurchaseOrderItemsController : BaseApiController
{
    private readonly IPurchaseOrderItemService _purchaseOrderItemService;

    public PurchaseOrderItemsController(IPurchaseOrderItemService purchaseOrderItemService)
    {
        _purchaseOrderItemService = purchaseOrderItemService;
    }

    /// <summary>Retrieves all line items for a purchase order.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PurchaseOrderItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPoNo([FromQuery] string poNo, CancellationToken cancellationToken)
    {
        var items = await _purchaseOrderItemService.GetByPoNoAsync(poNo, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PurchaseOrderItemDto>>.SuccessResponse(items));
    }

    /// <summary>Retrieves a single line item by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _purchaseOrderItemService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderItemDto>.SuccessResponse(item));
    }

    /// <summary>Adds a line item to an existing open purchase order and recomputes its TotalAmount.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderItemDto request, CancellationToken cancellationToken)
    {
        var item = await _purchaseOrderItemService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            ApiResponse<PurchaseOrderItemDto>.SuccessResponse(item, "Line item added successfully."));
    }

    /// <summary>Updates a line item's quantity/cost. Only allowed while the parent order is Open and nothing has been received against this line.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderItemDto request, CancellationToken cancellationToken)
    {
        var item = await _purchaseOrderItemService.UpdateAsync(id, request, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderItemDto>.SuccessResponse(item, "Line item updated successfully."));
    }

    /// <summary>Removes a line item. Only allowed while the parent order is Open, nothing has been received against it, and it is not the order's last remaining line.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _purchaseOrderItemService.DeleteAsync(id, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Line item deleted successfully."));
    }
}