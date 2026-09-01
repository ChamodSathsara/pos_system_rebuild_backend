using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Purchase;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Purchase order endpoints. Orders are created with their line items together, are only
/// editable while Open, and become locked once any receiving has happened against them.
/// </summary>
[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public class PurchaseOrdersController : BaseApiController
{
    private readonly IPurchaseOrderService _purchaseOrderService;

    public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
    {
        _purchaseOrderService = purchaseOrderService;
    }

    /// <summary>Searches purchase orders, optionally filtered by vendor, branch, status, or PO date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PurchaseOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] int? vendorId,
        [FromQuery] string? branchCode,
        [FromQuery] PurchaseOrderStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var orders = await _purchaseOrderService.SearchAsync(vendorId, branchCode, status, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PurchaseOrderDto>>.SuccessResponse(orders));
    }

    /// <summary>Retrieves a single purchase order, including its line items.</summary>
    [HttpGet("{poNo}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string poNo, CancellationToken cancellationToken)
    {
        var order = await _purchaseOrderService.GetByIdAsync(poNo, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(order));
    }

    /// <summary>
    /// Creates a purchase order with its line items. If PoNo is omitted, one is generated
    /// automatically (e.g. PO000001). Vendor must exist and be active; every item's ItemCode must exist.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto request, CancellationToken cancellationToken)
    {
        var order = await _purchaseOrderService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { poNo = order.PoNo },
            ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Purchase order created successfully."));
    }

    /// <summary>Replaces an open purchase order's header and line items. Blocked once anything has been received against it.</summary>
    [HttpPut("{poNo}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string poNo, [FromBody] UpdatePurchaseOrderDto request, CancellationToken cancellationToken)
    {
        var order = await _purchaseOrderService.UpdateAsync(poNo, request, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Purchase order updated successfully."));
    }

    /// <summary>Records a manual approval decision against an open order.</summary>
    [HttpPost("{poNo}/approve")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(string poNo, [FromBody] CancelPurchaseOrderDto request, CancellationToken cancellationToken)
    {
        var order = await _purchaseOrderService.ApproveAsync(poNo, request.Remarks, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Purchase order approved."));
    }

    /// <summary>Records a manual rejection decision against an open order.</summary>
    [HttpPost("{poNo}/reject")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(string poNo, [FromBody] CancelPurchaseOrderDto request, CancellationToken cancellationToken)
    {
        var order = await _purchaseOrderService.RejectAsync(poNo, request.Remarks, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Purchase order rejected."));
    }

    /// <summary>Cancels an order. Only allowed while Open and nothing has been received against it.</summary>
    [HttpPost("{poNo}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(string poNo, [FromBody] CancelPurchaseOrderDto request, CancellationToken cancellationToken)
    {
        var order = await _purchaseOrderService.CancelAsync(poNo, request, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(order, "Purchase order cancelled."));
    }

    /// <summary>Deletes a purchase order. Only allowed while Open, with no GRNs and nothing received.</summary>
    [HttpDelete("{poNo}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string poNo, CancellationToken cancellationToken)
    {
        await _purchaseOrderService.DeleteAsync(poNo, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Purchase order deleted successfully."));
    }
}