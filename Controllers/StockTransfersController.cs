using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Stock;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>Internal main-warehouse to branch-warehouse stock requests, dispatches and receipts.</summary>
[ApiController]
[Route("api/stock-transfers")]
[Authorize]
public class StockTransfersController(IStockTransferService service) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? sourceWarehouseCode, [FromQuery] string? destinationWarehouseCode, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<StockTransferRequestDto>>.SuccessResponse(await service.GetRequestsAsync(sourceWarehouseCode, destinationWarehouseCode, ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockTransferRequestDto request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, CurrentUserCode, CurrentBranchCode, CurrentRole, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<StockTransferRequestDto>.SuccessResponse(result, "Stock request submitted to the main warehouse."));
    }

    [HttpPost("{id:long}/accept")]
    [Authorize(Roles = "Admin,Manager,InventoryClerk")]
    public async Task<IActionResult> Accept(long id, [FromBody] AcceptStockTransferRequestDto request, CancellationToken ct) => Ok(ApiResponse<StockTransferRequestDto>.SuccessResponse(await service.AcceptAsync(id, request, CurrentUserCode, CurrentWarehouseCode, CurrentRole, ct), "Stock request accepted and added to the dispatch queue."));

    [HttpPost("{id:long}/dispatch")]
    [Authorize(Roles = "Admin,Manager,InventoryClerk")]
    public async Task<IActionResult> Dispatch(long id, [FromBody] CreateStockTransferDispatchDto request, CancellationToken ct) => StatusCode(StatusCodes.Status201Created, ApiResponse<StockTransferDispatchDto>.SuccessResponse(await service.DispatchAsync(id, request, CurrentUserCode, CurrentWarehouseCode, CurrentRole, ct), "Stock dispatched. Delivery note is ready."));

    /// <summary>Central InventoryClerk proposes a transfer. This does not reduce central stock.</summary>
    [HttpPost("direct")]
    [Authorize(Roles = "Admin,Manager,InventoryClerk")]
    public async Task<IActionResult> CreateDirect([FromBody] CreateDirectTransferProposalDto request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<StockTransferRequestDto>.SuccessResponse(
            await service.CreateDirectProposalAsync(request, CurrentUserCode, CurrentWarehouseCode, CurrentRole, ct),
            "Transfer proposal sent to the destination branch for acceptance."));

    /// <summary>Destination branch accepts a central warehouse transfer proposal, placing it in the central dispatch queue.</summary>
    [HttpPost("{id:long}/branch-accept")]
    public async Task<IActionResult> BranchAccept(long id, [FromBody] BranchTransferDecisionDto request, CancellationToken ct) =>
        Ok(ApiResponse<StockTransferRequestDto>.SuccessResponse(
            await service.BranchAcceptAsync(id, request, CurrentUserCode, CurrentBranchCode, CurrentRole, ct),
            "Transfer accepted. It is now in the main warehouse dispatch queue."));

    [HttpPost("dispatches/{dispatchId:long}/receive")]
    public async Task<IActionResult> Receive(long dispatchId, [FromBody] ReceiveStockTransferDispatchDto request, CancellationToken ct) => StatusCode(StatusCodes.Status201Created, ApiResponse<StockTransferReceiptDto>.SuccessResponse(await service.ReceiveAsync(dispatchId, request, CurrentUserCode, CurrentBranchCode, CurrentRole, ct), "Delivery received and branch stock updated."));

    [HttpGet("dispatches/{dispatchId:long}/delivery-note.pdf")]
    public async Task<IActionResult> DeliveryNote(long dispatchId, CancellationToken ct)
    {
        var report = await service.GetDeliveryNotePdfAsync(dispatchId, ct);
        return File(report.Content, "application/pdf", report.FileName);
    }

    [HttpGet("receipts/{receiptId:long}/receipt.pdf")]
    public async Task<IActionResult> Receipt(long receiptId, CancellationToken ct)
    {
        var report = await service.GetReceiptPdfAsync(receiptId, ct);
        return File(report.Content, "application/pdf", report.FileName);
    }
}
