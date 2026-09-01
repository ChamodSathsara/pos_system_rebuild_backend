using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Organization;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Warehouse management endpoints.
/// </summary>
[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController : BaseApiController
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    /// <summary>
    /// Retrieves warehouses, optionally filtered by branch code.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WarehouseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? branchCode, CancellationToken cancellationToken)
    {
        var warehouses = await _warehouseService.GetAllAsync(branchCode, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WarehouseDto>>.SuccessResponse(warehouses));
    }

    /// <summary>
    /// Retrieves a single warehouse by code.
    /// </summary>
    [HttpGet("{warehouseCode}")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string warehouseCode, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.GetByCodeAsync(warehouseCode, cancellationToken);
        return Ok(ApiResponse<WarehouseDto>.SuccessResponse(warehouse));
    }

    /// <summary>
    /// Creates a new warehouse.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetByCode),
            new { warehouseCode = warehouse.WarehouseCode },
            ApiResponse<WarehouseDto>.SuccessResponse(warehouse, "Warehouse created successfully."));
    }

    /// <summary>
    /// Updates an existing warehouse.
    /// </summary>
    [HttpPut("{warehouseCode}")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string warehouseCode, [FromBody] UpdateWarehouseDto request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.UpdateAsync(warehouseCode, request, cancellationToken);
        return Ok(ApiResponse<WarehouseDto>.SuccessResponse(warehouse, "Warehouse updated successfully."));
    }

    /// <summary>
    /// Deletes a warehouse.
    /// </summary>
    [HttpDelete("{warehouseCode}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string warehouseCode, CancellationToken cancellationToken)
    {
        await _warehouseService.DeleteAsync(warehouseCode, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Warehouse deleted successfully."));
    }
}
