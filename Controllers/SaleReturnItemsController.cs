using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Sale;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Read-only access to sale return line items. Lines are only ever created as part of posting a
/// sale return (see SaleReturnsController.Create) since each one drives a stock movement - there
/// is no standalone create/update endpoint here.
/// </summary>
[ApiController]
[Route("api/sale-return-items")]
[Authorize]
public class SaleReturnItemsController : BaseApiController
{
    private readonly ISaleReturnItemService _saleReturnItemService;

    public SaleReturnItemsController(ISaleReturnItemService saleReturnItemService)
    {
        _saleReturnItemService = saleReturnItemService;
    }

    /// <summary>Lists the line items for a given sale return.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SaleReturnItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByReturnNo([FromQuery] string returnNo, CancellationToken cancellationToken)
    {
        var items = await _saleReturnItemService.GetByReturnNoAsync(returnNo, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SaleReturnItemDto>>.SuccessResponse(items));
    }

    /// <summary>Retrieves a single sale return line item by its Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SaleReturnItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _saleReturnItemService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SaleReturnItemDto>.SuccessResponse(item));
    }
}
