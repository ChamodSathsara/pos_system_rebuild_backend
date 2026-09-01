using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Stock;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Stock batch endpoints. Creating a batch here is how stock gets received - it raises an "In"
/// StockMovement and increases the parent stock line's quantity automatically.
/// </summary>
[ApiController]
[Route("api/stock-batches")]
[Authorize]
public class StockBatchesController : BaseApiController
{
    private readonly IStockBatchService _stockBatchService;

    public StockBatchesController(IStockBatchService stockBatchService)
    {
        _stockBatchService = stockBatchService;
    }

    /// <summary>Retrieves all batches for a stock line.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StockBatchDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStockId([FromQuery] int stockId, CancellationToken cancellationToken)
    {
        var batches = await _stockBatchService.GetByStockIdAsync(stockId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StockBatchDto>>.SuccessResponse(batches));
    }

    /// <summary>Retrieves a single batch by id.</summary>
    [HttpGet("{batchId:long}")]
    [ProducesResponseType(typeof(ApiResponse<StockBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long batchId, CancellationToken cancellationToken)
    {
        var batch = await _stockBatchService.GetByIdAsync(batchId, cancellationToken);
        return Ok(ApiResponse<StockBatchDto>.SuccessResponse(batch));
    }

    /// <summary>Receives new stock into a batch. Raises an "In" movement and increases the parent stock line's CurrentQty.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StockBatchDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateStockBatchDto request, CancellationToken cancellationToken)
    {
        var batch = await _stockBatchService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { batchId = batch.BatchId },
            ApiResponse<StockBatchDto>.SuccessResponse(batch, "Stock batch received successfully."));
    }

    /// <summary>
    /// Updates batch metadata. Moving Status to Expired/Damaged/Blocked automatically writes off
    /// any remaining AvailableQty via a generated adjustment movement, keeping stock in sync.
    /// </summary>
    [HttpPut("{batchId:long}")]
    [ProducesResponseType(typeof(ApiResponse<StockBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long batchId, [FromBody] UpdateStockBatchDto request, CancellationToken cancellationToken)
    {
        var batch = await _stockBatchService.UpdateAsync(batchId, request, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<StockBatchDto>.SuccessResponse(batch, "Stock batch updated successfully."));
    }

    /// <summary>Deletes a batch. Only allowed while it is untouched (nothing consumed or adjusted against it yet).</summary>
    [HttpDelete("{batchId:long}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long batchId, CancellationToken cancellationToken)
    {
        await _stockBatchService.DeleteAsync(batchId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Stock batch deleted successfully."));
    }
}
