using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Sale;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Read-only access to sale line items. Lines are only ever created as part of posting a sale
/// (see SalesController.Create) since each one drives a stock movement - there is no standalone
/// create/update endpoint here.
/// </summary>
[ApiController]
[Route("api/sale-items")]
[Authorize]
public class SaleItemsController : BaseApiController
{
    private readonly ISaleItemService _saleItemService;

    public SaleItemsController(ISaleItemService saleItemService)
    {
        _saleItemService = saleItemService;
    }

    /// <summary>Lists the line items for a given invoice.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SaleItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByInvoiceNo([FromQuery] string invoiceNo, CancellationToken cancellationToken)
    {
        var items = await _saleItemService.GetByInvoiceNoAsync(invoiceNo, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SaleItemDto>>.SuccessResponse(items));
    }

    /// <summary>Retrieves a single sale line item by its Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SaleItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _saleItemService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SaleItemDto>.SuccessResponse(item));
    }
}
