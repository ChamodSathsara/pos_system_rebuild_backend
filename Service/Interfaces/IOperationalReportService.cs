using PosApi.DTOs.Reports;
using PosApi.Models.Enums;

namespace PosApi.Service.Interfaces;

public interface IOperationalReportService
{
    Task<CurrentStockReportDto> GetCurrentStockAsync(
        string? branchCode,
        string? warehouseCode,
        string? itemCode,
        int? categoryId,
        bool onlyAvailable,
        bool onlyBelowReorderLevel,
        CancellationToken cancellationToken = default);

    Task<StockMovementReportDto> GetStockMovementsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? warehouseCode,
        string? itemCode,
        StockMovementType? movementType,
        StockReferenceType? referenceType,
        string? referenceNo,
        CancellationToken cancellationToken = default);

    Task<PurchaseReportDto> GetPurchasesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        int? vendorId,
        string? itemCode,
        PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default);

    Task<ExpenseReportDto> GetExpensesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        int? categoryId,
        string? paidBy,
        CancellationToken cancellationToken = default);

    Task<DamageItemReportDto> GetDamageItemsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? warehouseCode,
        string? itemCode,
        DamageItemStatus? status,
        CancellationToken cancellationToken = default);

    Task<ProfitReportDto> GetProfitAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        CancellationToken cancellationToken = default);
}
