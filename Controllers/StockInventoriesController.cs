using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Stock;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Stock-on-hand endpoints, one row per item/branch/warehouse combination. Quantities are driven
/// by StockBatch receipts and StockMovement entries - use ReconcileAsync (PUT) to recompute
/// CurrentQty from the batches on hand rather than editing it directly.
/// </summary>
[ApiController]
[Route("api/stock-inventories")]
[Authorize]
public class StockInventoriesController : BaseApiController
{
    private readonly IStockInventoryService _stockInventoryService;

    public StockInventoriesController(IStockInventoryService stockInventoryService)
    {
        _stockInventoryService = stockInventoryService;
    }

    /// <summary>Searches stock lines, optionally filtered by item, branch, warehouse, or items at/below reorder level.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StockInventoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? itemCode,
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] bool onlyBelowReorderLevel,
        CancellationToken cancellationToken)
    {
        var stock = await _stockInventoryService.SearchAsync(itemCode, branchCode, warehouseCode, onlyBelowReorderLevel, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StockInventoryDto>>.SuccessResponse(stock));
    }

    /// <summary>Retrieves a single stock line by id.</summary>
    [HttpGet("{stockId:int}")]
    [ProducesResponseType(typeof(ApiResponse<StockInventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int stockId, CancellationToken cancellationToken)
    {
        var stock = await _stockInventoryService.GetByIdAsync(stockId, cancellationToken);
        return Ok(ApiResponse<StockInventoryDto>.SuccessResponse(stock));
    }

    /// <summary>Registers a new item/branch/warehouse stock line, starting at zero quantity. Receive stock into it via the batches endpoint.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StockInventoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateStockInventoryDto request, CancellationToken cancellationToken)
    {
        var stock = await _stockInventoryService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { stockId = stock.StockId },
            ApiResponse<StockInventoryDto>.SuccessResponse(stock, "Stock line created successfully."));
    }

    /// <summary>Recounts CurrentQty from the sum of the stock line's batches and refreshes LastUpdated. CurrentQty cannot be edited directly.</summary>
    [HttpPut("{stockId:int}")]
    [ProducesResponseType(typeof(ApiResponse<StockInventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reconcile(int stockId, CancellationToken cancellationToken)
    {
        var stock = await _stockInventoryService.ReconcileAsync(stockId, cancellationToken);
        return Ok(ApiResponse<StockInventoryDto>.SuccessResponse(stock, "Stock line reconciled successfully."));
    }

    /// <summary>Deletes a stock line. Only allowed while its quantity is zero and it has no batches.</summary>
    [HttpDelete("{stockId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int stockId, CancellationToken cancellationToken)
    {
        await _stockInventoryService.DeleteAsync(stockId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Stock line deleted successfully."));
    }
}
