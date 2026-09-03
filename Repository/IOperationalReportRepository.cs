using PosApi.Models.Entities;
using PosApi.Models.Enums;

namespace PosApi.Repository;

public interface IOperationalReportRepository
{
    Task<IReadOnlyList<StockInventory>> GetCurrentStockAsync(
        string? branchCode,
        string? warehouseCode,
        string? itemCode,
        int? categoryId,
        bool onlyAvailable,
        bool onlyBelowReorderLevel,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockMovement>> GetStockMovementsAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        string? warehouseCode,
        string? itemCode,
        StockMovementType? movementType,
        StockReferenceType? referenceType,
        string? referenceNo,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        int? vendorId,
        string? itemCode,
        PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GrnMaster>> GetGrnsAsync(
        string? branchCode,
        int? vendorId,
        string? itemCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GrnReturn>> GetGrnReturnsAsync(
        string? branchCode,
        int? vendorId,
        string? itemCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Expense>> GetExpensesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        int? categoryId,
        string? paidBy,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sale>> GetSalesAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SaleReturn>> GetSaleReturnsAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockMovement>> GetProfitMovementsAsync(
        DateTime fromDate,
        DateTime toDate,
        string? branchCode,
        CancellationToken cancellationToken = default);
}