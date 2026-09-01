using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Sale;
using PosApi.Helpers;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// POS sale endpoints. A sale is created complete with its line items in one call - stock is
/// drawn down immediately since it represents goods that have already left the store. Once
/// posted, a sale can only be voided (Cancel) or partially reversed via SaleReturns; it is never
/// edited in place.
/// </summary>
[ApiController]
[Route("api/sales")]
[Authorize]
public class SalesController : BaseApiController
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    /// <summary>Searches sales, optionally filtered by branch, customer, status, or sale date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SaleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? branchCode,
        [FromQuery] string? customerCode,
        [FromQuery] SaleStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var sales = await _saleService.SearchAsync(branchCode, customerCode, status, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SaleDto>>.SuccessResponse(sales));
    }

    /// <summary>Retrieves a single sale, including its line items.</summary>
    [HttpGet("{invoiceNo}")]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string invoiceNo, CancellationToken cancellationToken)
    {
        var sale = await _saleService.GetByIdAsync(invoiceNo, cancellationToken);
        return Ok(ApiResponse<SaleDto>.SuccessResponse(sale));
    }

    /// <summary>
    /// Posts a completed sale with its line items. If InvoiceNo is omitted, one is generated
    /// automatically (e.g. INV000001). Stock is drawn down FIFO across the branch's available
    /// batches for every item; insufficient stock fails the whole sale. Any initial payments
    /// supplied are recorded against the new invoice as well.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSaleDto request, CancellationToken cancellationToken)
    {
        var sale = await _saleService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { invoiceNo = sale.InvoiceNo },
            ApiResponse<SaleDto>.SuccessResponse(sale, "Sale posted successfully."));
    }

    /// <summary>
    /// Builds the printable invoice/receipt for a completed sale, sized for an 80mm thermal
    /// printer (72-76mm printable width, 2-4mm margins, monospace font, bold/larger TOTAL).
    /// Returns structured JSON by default; pass <c>format=text</c> for a raw monospace receipt
    /// ready to send straight to a receipt printer, or <c>format=html</c> for a printable page
    /// (opens the browser print dialog) styled to the same layout.
    /// </summary>
    [HttpGet("{invoiceNo}/invoice")]
    [ProducesResponseType(typeof(ApiResponse<SaleInvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(
        string invoiceNo,
        CancellationToken cancellationToken,
        [FromQuery] string format = "json")
    {
        var invoice = await _saleService.GetInvoiceAsync(invoiceNo, cancellationToken);

        switch (format.Trim().ToLowerInvariant())
        {
            case "text":
                return Content(InvoicePrintFormatter.ToText(invoice), "text/plain");
            case "html":
                return Content(InvoicePrintFormatter.ToHtml(invoice), "text/html");
            default:
                return Ok(ApiResponse<SaleInvoiceDto>.SuccessResponse(invoice));
        }
    }

    /// <summary>Voids a completed sale, restoring the stock it drew down. Blocked once any returns or payments have been recorded against it.</summary>
    [HttpPost("{invoiceNo}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(string invoiceNo, CancellationToken cancellationToken)
    {
        var sale = await _saleService.CancelAsync(invoiceNo, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<SaleDto>.SuccessResponse(sale, "Sale cancelled and stock restored."));
    }
}
