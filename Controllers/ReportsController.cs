using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.Constants;
using PosApi.Exceptions;
using PosApi.Helpers;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Sales reporting endpoints. Every report supports three output shapes via <c>?format=</c>:
/// <c>json</c> (default, for API/frontend use), <c>pdf</c> (downloadable/printable) and
/// <c>excel</c> (downloadable .xlsx).
///
/// Access control: Admin and Manager can see every branch and may filter by branchCode.
/// Branch_Manager is pinned to the branch on their own account - any branchCode they pass must
/// match it, and it is applied automatically when they omit it. Cashier has no report access at
/// all.
/// </summary>
[ApiController]
[Route("api/reports/sales")]
[Authorize]
public class ReportsController : BaseApiController
{
    private readonly ISalesReportService _salesReportService;

    public ReportsController(ISalesReportService salesReportService)
    {
        _salesReportService = salesReportService;
    }

    /// <summary>Daily total sales, invoice count, gross/subtotal, discounts, returns, net sales and payment summary - one row per day in the range.</summary>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDailySalesReport(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? cashierCode,
        [FromQuery] string? customerCode,
        [FromQuery] string format = "json",
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(fromDate, toDate);
        var effectiveBranchCode = ResolveEffectiveBranchCode(branchCode);

        var report = await _salesReportService.GetDailySalesReportAsync(
            fromDate, toDate, effectiveBranchCode, cashierCode, customerCode, cancellationToken);

        return format.Trim().ToLowerInvariant() switch
        {
            "pdf" => File(SalesReportPdfExporter.ExportDailySalesReport(report), "application/pdf", $"daily-sales-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.pdf"),
            "excel" or "xlsx" => File(SalesReportExcelExporter.ExportDailySalesReport(report), ExcelContentType, $"daily-sales-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx"),
            _ => Ok(ApiResponse<object>.SuccessResponse(report))
        };
    }

    /// <summary>Sales summary rolled up across the date range - total invoices, quantity sold, gross sales, discounts, returns and net sales.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSalesSummaryReport(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? cashierCode,
        [FromQuery] string? customerCode,
        [FromQuery] string format = "json",
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(fromDate, toDate);
        var effectiveBranchCode = ResolveEffectiveBranchCode(branchCode);

        var report = await _salesReportService.GetSalesSummaryReportAsync(
            fromDate, toDate, effectiveBranchCode, cashierCode, customerCode, cancellationToken);

        return format.Trim().ToLowerInvariant() switch
        {
            "pdf" => File(SalesReportPdfExporter.ExportSalesSummaryReport(report), "application/pdf", $"sales-summary-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.pdf"),
            "excel" or "xlsx" => File(SalesReportExcelExporter.ExportSalesSummaryReport(report), ExcelContentType, $"sales-summary-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx"),
            _ => Ok(ApiResponse<object>.SuccessResponse(report))
        };
    }

    /// <summary>Per-item breakdown - quantity sold, selling amount, discount, return quantity/amount and net sales amount.</summary>
    [HttpGet("item-wise")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetItemWiseSalesReport(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? cashierCode,
        [FromQuery] string? customerCode,
        [FromQuery] string? itemCode,
        [FromQuery] int? categoryId,
        [FromQuery] string format = "json",
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(fromDate, toDate);
        var effectiveBranchCode = ResolveEffectiveBranchCode(branchCode);

        var report = await _salesReportService.GetItemWiseSalesReportAsync(
            fromDate, toDate, effectiveBranchCode, cashierCode, customerCode, itemCode, categoryId, cancellationToken);

        return format.Trim().ToLowerInvariant() switch
        {
            "pdf" => File(SalesReportPdfExporter.ExportItemWiseSalesReport(report), "application/pdf", $"item-wise-sales-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.pdf"),
            "excel" or "xlsx" => File(SalesReportExcelExporter.ExportItemWiseSalesReport(report), ExcelContentType, $"item-wise-sales-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx"),
            _ => Ok(ApiResponse<object>.SuccessResponse(report))
        };
    }

    /// <summary>Sales returns in the period - return number/date, original invoice, item details, returned quantity/amount and reason.</summary>
    [HttpGet("returns")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSalesReturnReport(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? cashierCode,
        [FromQuery] string? customerCode,
        [FromQuery] string? itemCode,
        [FromQuery] string format = "json",
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(fromDate, toDate);
        var effectiveBranchCode = ResolveEffectiveBranchCode(branchCode);

        var report = await _salesReportService.GetSalesReturnReportAsync(
            fromDate, toDate, effectiveBranchCode, cashierCode, customerCode, itemCode, cancellationToken);

        return format.Trim().ToLowerInvariant() switch
        {
            "pdf" => File(SalesReportPdfExporter.ExportSalesReturnReport(report), "application/pdf", $"sales-return-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.pdf"),
            "excel" or "xlsx" => File(SalesReportExcelExporter.ExportSalesReturnReport(report), ExcelContentType, $"sales-return-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx"),
            _ => Ok(ApiResponse<object>.SuccessResponse(report))
        };
    }

    // -----------------------------------------------------------------------------------------

    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static void ValidateDateRange(DateOnly fromDate, DateOnly toDate)
    {
        if (toDate < fromDate)
        {
            throw new BadRequestException("toDate must be on or after fromDate.");
        }
    }

    /// <summary>
    /// Enforces the report access-control rules and returns the branchCode that should actually
    /// be used for the query: Admin/Manager get whatever the caller asked for (including "all
    /// branches" when omitted); Branch_Manager is force-pinned to their own branch; Cashier is
    /// rejected outright.
    /// </summary>
    private string? ResolveEffectiveBranchCode(string? requestedBranchCode)
    {
        var role = CurrentRole;

        if (string.Equals(role, RoleConstants.Cashier, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenAppException("Cashiers do not have access to sales reports.");
        }

        if (string.Equals(role, RoleConstants.BranchManager, StringComparison.OrdinalIgnoreCase))
        {
            var ownBranchCode = CurrentBranchCode;
            if (string.IsNullOrWhiteSpace(ownBranchCode))
            {
                throw new ForbiddenAppException("Your account has no branch assigned; contact an administrator.");
            }

            if (!string.IsNullOrWhiteSpace(requestedBranchCode)
                && !string.Equals(requestedBranchCode, ownBranchCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenAppException("Branch managers can only view reports for their own branch.");
            }

            return ownBranchCode;
        }

        if (string.Equals(role, RoleConstants.Admin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, RoleConstants.Manager, StringComparison.OrdinalIgnoreCase))
        {
            return requestedBranchCode;
        }

        // Any other/unrecognised role: deny by default rather than silently granting access.
        throw new ForbiddenAppException("Your role does not have access to sales reports.");
    }
}
