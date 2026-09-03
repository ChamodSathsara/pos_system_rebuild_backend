using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.Constants;
using PosApi.DTOs.Reports;
using PosApi.Exceptions;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class OperationalReportsController : BaseApiController
{
    private readonly IOperationalReportService _service;

    public OperationalReportsController(
        IOperationalReportService service)
    {
        _service = service;
    }

    [HttpGet("stock/current")]
    public async Task<IActionResult> GetCurrentStock(
        [FromQuery] string? branchCode,
        [FromQuery] string? warehouseCode,
        [FromQuery] string? itemCode,
        [FromQuery] int? categoryId,
        [FromQuery] bool onlyAvailable = false,
        [FromQuery] bool onlyBelowReorderLevel = false,
        CancellationToken cancellationToken = default)
    {
        var effectiveBranch =
            ResolveBranch(branchCode);

        var report = await _service.GetCurrentStockAsync(
            effectiveBranch,
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

    [HttpGet("stock/movements")]
    public async Task<IActionResult> GetStockMovements(
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
        ValidateDates(fromDate, toDate);

        var report = await _service.GetStockMovementsAsync(
            fromDate,
            toDate,
            ResolveBranch(branchCode),
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

    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchases(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] int? vendorId,
        [FromQuery] string? itemCode,
        [FromQuery] PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        ValidateDates(fromDate, toDate);

        var report = await _service.GetPurchasesAsync(
            fromDate,
            toDate,
            ResolveBranch(branchCode),
            vendorId,
            itemCode,
            status,
            cancellationToken);

        return Ok(
            ApiResponse<PurchaseReportDto>
                .SuccessResponse(report));
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        [FromQuery] int? categoryId,
        [FromQuery] string? paidBy,
        CancellationToken cancellationToken = default)
    {
        ValidateDates(fromDate, toDate);

        var report = await _service.GetExpensesAsync(
            fromDate,
            toDate,
            ResolveBranch(branchCode),
            categoryId,
            paidBy,
            cancellationToken);

        return Ok(
            ApiResponse<ExpenseReportDto>
                .SuccessResponse(report));
    }

    [HttpGet("profit")]
    public async Task<IActionResult> GetProfit(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string? branchCode,
        CancellationToken cancellationToken = default)
    {
        ValidateDates(fromDate, toDate);

        var report = await _service.GetProfitAsync(
            fromDate,
            toDate,
            ResolveBranch(branchCode),
            cancellationToken);

        return Ok(
            ApiResponse<ProfitReportDto>
                .SuccessResponse(report));
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