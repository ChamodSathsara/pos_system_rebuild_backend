using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Sale;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Sale return endpoints. A return is posted complete with its line items against a completed
/// sale, restoring stock to the same batches the original sale drew from (FIFO) in one
/// transaction. Returns are append-only records, mirroring GrnReturns on the purchasing side.
/// </summary>
[ApiController]
[Route("api/sale-returns")]
[Authorize]
public class SaleReturnsController : BaseApiController
{
    private readonly ISaleReturnService _saleReturnService;

    public SaleReturnsController(ISaleReturnService saleReturnService)
    {
        _saleReturnService = saleReturnService;
    }

    /// <summary>Searches sale returns, optionally filtered by invoice or return date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SaleReturnDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? invoiceNo,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var returns = await _saleReturnService.SearchAsync(invoiceNo, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SaleReturnDto>>.SuccessResponse(returns));
    }

    /// <summary>Retrieves a single sale return, including its line items.</summary>
    [HttpGet("{returnNo}")]
    [ProducesResponseType(typeof(ApiResponse<SaleReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string returnNo, CancellationToken cancellationToken)
    {
        var saleReturn = await _saleReturnService.GetByIdAsync(returnNo, cancellationToken);
        return Ok(ApiResponse<SaleReturnDto>.SuccessResponse(saleReturn));
    }

    /// <summary>
    /// Posts a return against a completed sale. If ReturnNo is omitted, one is generated
    /// automatically (e.g. SRT000001). Each line's quantity is checked against what's still
    /// returnable (sold minus already returned) for that item on the invoice, and stock is
    /// restored to the batches the original sale drew from.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SaleReturnDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSaleReturnDto request, CancellationToken cancellationToken)
    {
        var saleReturn = await _saleReturnService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { returnNo = saleReturn.ReturnNo },
            ApiResponse<SaleReturnDto>.SuccessResponse(saleReturn, "Sale return posted successfully."));
    }
}
