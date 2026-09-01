using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Stock;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Stock movement endpoints - an append-mostly audit trail of every quantity change against a
/// batch. POST records a manual adjustment; PUT only allows correcting descriptive metadata;
/// DELETE only allows reversing the single most recent adjustment movement for a batch.
/// </summary>
[ApiController]
[Route("api/stock-movements")]
[Authorize]
public class StockMovementsController : BaseApiController
{
    private readonly IStockMovementService _stockMovementService;

    public StockMovementsController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    /// <summary>Searches movements, optionally filtered by stock line, batch, or reference number.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StockMovementDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] int? stockId,
        [FromQuery] long? batchId,
        [FromQuery] string? referenceNo,
        CancellationToken cancellationToken)
    {
        var movements = await _stockMovementService.SearchAsync(stockId, batchId, referenceNo, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StockMovementDto>>.SuccessResponse(movements));
    }

    /// <summary>Retrieves a single movement by id.</summary>
    [HttpGet("{movementId:long}")]
    [ProducesResponseType(typeof(ApiResponse<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long movementId, CancellationToken cancellationToken)
    {
        var movement = await _stockMovementService.GetByIdAsync(movementId, cancellationToken);
        return Ok(ApiResponse<StockMovementDto>.SuccessResponse(movement));
    }

    /// <summary>Records a manual adjustment movement against a batch. Qty is signed and keeps the batch/stock line in sync.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StockMovementDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementDto request, CancellationToken cancellationToken)
    {
        var movement = await _stockMovementService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { movementId = movement.MovementId },
            ApiResponse<StockMovementDto>.SuccessResponse(movement, "Stock movement recorded successfully."));
    }

    /// <summary>Corrects a movement's descriptive metadata (ReferenceNo/Remarks). Quantities are immutable.</summary>
    [HttpPut("{movementId:long}")]
    [ProducesResponseType(typeof(ApiResponse<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long movementId, [FromBody] UpdateStockMovementDto request, CancellationToken cancellationToken)
    {
        var movement = await _stockMovementService.UpdateAsync(movementId, request, cancellationToken);
        return Ok(ApiResponse<StockMovementDto>.SuccessResponse(movement, "Stock movement updated successfully."));
    }

    /// <summary>Reverses and deletes a movement. Only the most recently recorded adjustment movement for a batch can be removed.</summary>
    [HttpDelete("{movementId:long}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long movementId, CancellationToken cancellationToken)
    {
        await _stockMovementService.DeleteAsync(movementId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Stock movement deleted successfully."));
    }
}
