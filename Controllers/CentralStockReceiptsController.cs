using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Stock;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

[ApiController]
[Route("api/central-stock-receipts")]
[Authorize(Roles = "Admin,Manager,InventoryClerk")]
public class CentralStockReceiptsController(ICentralStockReceiptService service) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? warehouseCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<CentralStockReceiptDto>>.SuccessResponse(await service.SearchAsync(warehouseCode, fromDate, toDate, ct)));

    [HttpGet("{receiptId:long}")]
    public async Task<IActionResult> Get(long receiptId, CancellationToken ct) =>
        Ok(ApiResponse<CentralStockReceiptDto>.SuccessResponse(await service.GetByIdAsync(receiptId, ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCentralStockReceiptDto request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<CentralStockReceiptDto>.SuccessResponse(
            await service.CreateAsync(request, CurrentUserCode, CurrentWarehouseCode, CurrentRole, ct),
            "Central warehouse stock received successfully."));

    [HttpGet("{receiptId:long}/receipt.pdf")]
    public async Task<IActionResult> ReceiptPdf(long receiptId, CancellationToken ct)
    {
        var result = await service.GetReceiptPdfAsync(receiptId, ct);
        return File(result.Content, "application/pdf", result.FileName);
    }
}
