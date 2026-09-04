using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.Constants;
using PosApi.DTOs.Reports;
using PosApi.Exceptions;
using PosApi.Helpers;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class OperationalReportsController : BaseApiController
{
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IOperationalReportService _service;

    public OperationalReportsController(
        IOperationalReportService service)
    {
        _service = service;
    }

    // =====================================================
    // 1. CURRENT STOCK REPORT
    // =====================================================

    [HttpGet("stock/current")]
    public async Task<IActionResult> GetCurrentStockJson(
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] int? categoryId,
        [FromQuery] bool onlyAvailable = false,
        [FromQuery] bool onlyBelowReorderLevel = false,
        CancellationToken cancellationToken = default)
    {
        var report = await GetCurrentStockReport(
            branchCode,
            warehouseCode,
            itemCode,
            categoryId,
            onlyAvailable,
            onlyBelowReorderLevel,
            cancellationToken);

        return Ok(
            ApiResponse<CurrentStockReportDto>
                .SuccessResponse(report));
    }

    [HttpGet("stock/current/pdf")]
    public async Task<IActionResult> DownloadCurrentStockPdf(
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] int? categoryId,
        [FromQuery] bool onlyAvailable = false,
        [FromQuery] bool onlyBelowReorderLevel = false,
        CancellationToken cancellationToken = default)
    {
        var report = await GetCurrentStockReport(
            branchCode,
            warehouseCode,
            itemCode,
            categoryId,
            onlyAvailable,
            onlyBelowReorderLevel,
            cancellationToken);

        return File(
            OperationalReportPdfExporter
                .ExportCurrentStock(report),
            "application/pdf",
            $"current-stock-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("stock/current/excel")]
    public async Task<IActionResult> DownloadCurrentStockExcel(
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] int? categoryId,
        [FromQuery] bool onlyAvailable = false,
        [FromQuery] bool onlyBelowReorderLevel = false,
        CancellationToken cancellationToken = default)
    {
        var report = await GetCurrentStockReport(
            branchCode,
            warehouseCode,
            itemCode,
            categoryId,
            onlyAvailable,
            onlyBelowReorderLevel,
            cancellationToken);

        return File(
            OperationalReportExcelExporter
                .ExportCurrentStock(report),
            ExcelContentType,
            $"current-stock-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    // =====================================================
    // 2. STOCK MOVEMENT REPORT
    // =====================================================

    [HttpGet("stock/movements")]
    public async Task<IActionResult> GetStockMovementsJson(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] StockMovementType? movementType,
        [FromQuery] StockReferenceType? referenceType,
        [FromQuery] string? referenceNo,
        CancellationToken cancellationToken = default)
    {
        var report = await GetStockMovementReport(
            fromDate,
            toDate,
            branchCode,
            warehouseCode,
            itemCode,
            movementType,
            referenceType,
            referenceNo,
            cancellationToken);

        return Ok(
            ApiResponse<StockMovementReportDto>
                .SuccessResponse(report));
    }

    [HttpGet("stock/movements/pdf")]
    public async Task<IActionResult> DownloadStockMovementsPdf(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] StockMovementType? movementType,
        [FromQuery] StockReferenceType? referenceType,
        [FromQuery] string? referenceNo,
        CancellationToken cancellationToken = default)
    {
        var report = await GetStockMovementReport(
            fromDate,
            toDate,
            branchCode,
            warehouseCode,
            itemCode,
            movementType,
            referenceType,
            referenceNo,
            cancellationToken);

        return File(
            OperationalReportPdfExporter
                .ExportStockMovements(report),
            "application/pdf",
            $"stock-movements-{fromDate:yyyyMMdd}-" +
            $"{toDate:yyyyMMdd}.pdf");
    }

    [HttpGet("stock/movements/excel")]
    public async Task<IActionResult> DownloadStockMovementsExcel(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] StockMovementType? movementType,
        [FromQuery] StockReferenceType? referenceType,
        [FromQuery] string? referenceNo,
        CancellationToken cancellationToken = default)
    {
        var report = await GetStockMovementReport(
            fromDate,
            toDate,
            branchCode,
            warehouseCode,
            itemCode,
            movementType,
            referenceType,
            referenceNo,
            cancellationToken);

        return File(
            OperationalReportExcelExporter
                .ExportStockMovements(report),
            ExcelContentType,
            $"stock-movements-{fromDate:yyyyMMdd}-" +
            $"{toDate:yyyyMMdd}.xlsx");
    }

    // =====================================================
    // 3. PURCHASE / GRN REPORT
    // =====================================================

    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchasesJson(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] int? vendorId,
        [FromQuery] string? itemCode,
        [FromQuery] PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        var report = await GetPurchaseReport(
            fromDate,
            toDate,
            branchCode,
            vendorId,
            itemCode,
            status,
            cancellationToken);

        return Ok(
            ApiResponse<PurchaseReportDto>
                .SuccessResponse(report));
    }

    [HttpGet("purchases/pdf")]
    public async Task<IActionResult> DownloadPurchasesPdf(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] int? vendorId,
        [FromQuery] string? itemCode,
        [FromQuery] PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        var report = await GetPurchaseReport(
            fromDate,
            toDate,
            branchCode,
            vendorId,
            itemCode,
            status,
            cancellationToken);

        return File(
            OperationalReportPdfExporter
                .ExportPurchases(report),
            "application/pdf",
            $"purchase-report-{fromDate:yyyyMMdd}-" +
            $"{toDate:yyyyMMdd}.pdf");
    }

    [HttpGet("purchases/excel")]
    public async Task<IActionResult> DownloadPurchasesExcel(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] int? vendorId,
        [FromQuery] string? itemCode,
        [FromQuery] PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        var report = await GetPurchaseReport(
            fromDate,
            toDate,
            branchCode,
            vendorId,
            itemCode,
            status,
            cancellationToken);

        return File(
            OperationalReportExcelExporter
                .ExportPurchases(report),
            ExcelContentType,
            $"purchase-report-{fromDate:yyyyMMdd}-" +
            $"{toDate:yyyyMMdd}.xlsx");
    }

    // =====================================================
    // 4. EXPENSE REPORT
    // =====================================================

    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpensesJson(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] int? categoryId,
        [FromQuery] string? paidBy,
        CancellationToken cancellationToken = default)
    {
        var report = await GetExpenseReport(
            fromDate,
            toDate,
            branchCode,
            categoryId,
            paidBy,
            cancellationToken);

        return Ok(
            ApiResponse<ExpenseReportDto>
                .SuccessResponse(report));
    }

    [HttpGet("expenses/pdf")]
    public async Task<IActionResult> DownloadExpensesPdf(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] int? categoryId,
        [FromQuery] string? paidBy,
        CancellationToken cancellationToken = default)
    {
        var report = await GetExpenseReport(
            fromDate,
            toDate,
            branchCode,
            categoryId,
            paidBy,
            cancellationToken);

        return File(
            OperationalReportPdfExporter
                .ExportExpenses(report),
            "application/pdf",
            $"expense-report-{fromDate:yyyyMMdd}-" +
            $"{toDate:yyyyMMdd}.pdf");
    }

    [HttpGet("expenses/excel")]
    public async Task<IActionResult> DownloadExpensesExcel(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] int? categoryId,
        [FromQuery] string? paidBy,
        CancellationToken cancellationToken = default)
    {
        var report = await GetExpenseReport(
            fromDate,
            toDate,
            branchCode,
            categoryId,
            paidBy,
            cancellationToken);

        return File(
            OperationalReportExcelExporter
                .ExportExpenses(report),
            ExcelContentType,
            $"expense-report-{fromDate:yyyyMMdd}-" +
            $"{toDate:yyyyMMdd}.xlsx");
    }

    // =====================================================
    // 5. DAMAGED ITEMS REPORT
    // =====================================================

    [HttpGet("damage-items")]
    public async Task<IActionResult> GetDamageItemsJson(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] DamageItemStatus? status,
        CancellationToken cancellationToken = default)
    {
        var report = await GetDamageItemReport(fromDate, toDate, branchCode,
            warehouseCode, itemCode, status, cancellationToken);
        return Ok(ApiResponse<DamageItemReportDto>.SuccessResponse(report));
    }

    [HttpGet("damage-items/pdf")]
    public async Task<IActionResult> DownloadDamageItemsPdf(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] DamageItemStatus? status,
        CancellationToken cancellationToken = default)
    {
        var report = await GetDamageItemReport(fromDate, toDate, branchCode,
            warehouseCode, itemCode, status, cancellationToken);
        return File(OperationalReportPdfExporter.ExportDamageItems(report), "application/pdf",
            $"damage-items-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.pdf");
    }

    [HttpGet("damage-items/excel")]
    public async Task<IActionResult> DownloadDamageItemsExcel(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] DamageItemStatus? status,
        CancellationToken cancellationToken = default)
    {
        var report = await GetDamageItemReport(fromDate, toDate, branchCode,
            warehouseCode, itemCode, status, cancellationToken);
        return File(OperationalReportExcelExporter.ExportDamageItems(report), ExcelContentType,
            $"damage-items-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx");
    }

    // =====================================================
    // 6. PROFIT REPORT
    // =====================================================

    [HttpGet("profit")]
    public async Task<IActionResult> GetProfitJson(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        CancellationToken cancellationToken = default)
    {
        var report = await GetProfitReport(
            fromDate,
            toDate,
            branchCode,
            cancellationToken);

        return Ok(
            ApiResponse<ProfitReportDto>
                .SuccessResponse(report));
    }

    [HttpGet("profit/pdf")]
    public async Task<IActionResult> DownloadProfitPdf(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        CancellationToken cancellationToken = default)
    {
        var report = await GetProfitReport(
            fromDate,
            toDate,
            branchCode,
            cancellationToken);

        return File(
            OperationalReportPdfExporter
                .ExportProfit(report),
            "application/pdf",
            $"profit-report-{fromDate:yyyyMMdd}-" +
            $"{toDate:yyyyMMdd}.pdf");
    }

    [HttpGet("profit/excel")]
    public async Task<IActionResult> DownloadProfitExcel(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        CancellationToken cancellationToken = default)
    {
        var report = await GetProfitReport(
            fromDate,
            toDate,
            branchCode,
            cancellationToken);

        return File(
            OperationalReportExcelExporter
                .ExportProfit(report),
            ExcelContentType,
            $"profit-report-{fromDate:yyyyMMdd}-" +
            $"{toDate:yyyyMMdd}.xlsx");
    }

    // =====================================================
    // SHARED PRIVATE METHODS
    // =====================================================

    private Task<CurrentStockReportDto> GetCurrentStockReport(
        string? branchCode,
        string? warehouseCode,
        string? itemCode,
        int? categoryId,
        bool onlyAvailable,
        bool onlyBelowReorderLevel,
        CancellationToken cancellationToken)
    {
        return _service.GetCurrentStockAsync(
            ResolveBranch(branchCode),
            warehouseCode,
            itemCode,
            categoryId,
            onlyAvailable,
            onlyBelowReorderLevel,
            cancellationToken);
    }

    private Task<StockMovementReportDto> GetStockMovementReport(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? warehouseCode,
        string? itemCode,
        StockMovementType? movementType,
        StockReferenceType? referenceType,
        string? referenceNo,
        CancellationToken cancellationToken)
    {
        ValidateDates(fromDate, toDate);

        return _service.GetStockMovementsAsync(
            fromDate,
            toDate,
            ResolveBranch(branchCode),
            warehouseCode,
            itemCode,
            movementType,
            referenceType,
            referenceNo,
            cancellationToken);
    }

    private Task<PurchaseReportDto> GetPurchaseReport(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        int? vendorId,
        string? itemCode,
        PurchaseOrderStatus? status,
        CancellationToken cancellationToken)
    {
        ValidateDates(fromDate, toDate);

        return _service.GetPurchasesAsync(
            fromDate,
            toDate,
            ResolveBranch(branchCode),
            vendorId,
            itemCode,
            status,
            cancellationToken);
    }

    private Task<ExpenseReportDto> GetExpenseReport(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        int? categoryId,
        string? paidBy,
        CancellationToken cancellationToken)
    {
        ValidateDates(fromDate, toDate);

        return _service.GetExpensesAsync(
            fromDate,
            toDate,
            ResolveBranch(branchCode),
            categoryId,
            paidBy,
            cancellationToken);
    }

    private Task<ProfitReportDto> GetProfitReport(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        CancellationToken cancellationToken)
    {
        ValidateDates(fromDate, toDate);

        return _service.GetProfitAsync(
            fromDate,
            toDate,
            ResolveBranch(branchCode),
            cancellationToken);
    }

    private Task<DamageItemReportDto> GetDamageItemReport(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? warehouseCode,
        string? itemCode,
        DamageItemStatus? status,
        CancellationToken cancellationToken)
    {
        ValidateDates(fromDate, toDate);
        return _service.GetDamageItemsAsync(fromDate, toDate, ResolveBranch(branchCode),
            warehouseCode, itemCode, status, cancellationToken);
    }

    private string? ResolveBranch(
        string? requestedBranchCode)
    {
        if (string.Equals(
                CurrentRole,
                RoleConstants.Cashier,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenAppException(
                "Cashiers do not have report access.");
        }

        if (string.Equals(
                CurrentRole,
                RoleConstants.BranchManager,
                StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(CurrentBranchCode))
            {
                throw new ForbiddenAppException(
                    "Your account has no branch assigned.");
            }

            if (!string.IsNullOrWhiteSpace(
                    requestedBranchCode) &&
                !string.Equals(
                    requestedBranchCode,
                    CurrentBranchCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenAppException(
                    "You can only view your own branch.");
            }

            return CurrentBranchCode;
        }

        if (string.Equals(
                CurrentRole,
                RoleConstants.Admin,
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                CurrentRole,
                RoleConstants.Manager,
                StringComparison.OrdinalIgnoreCase))
        {
            return requestedBranchCode;
        }

        throw new ForbiddenAppException(
            "Your role does not have report access.");
    }

    private static void ValidateDates(
        DateOnly fromDate,
        DateOnly toDate)
    {
        if (toDate < fromDate)
        {
            throw new BadRequestException(
                "toDate must be on or after fromDate.");
        }
    }
}
