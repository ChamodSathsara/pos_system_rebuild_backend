using PosApi.DTOs.Reports;

namespace PosApi.Service.Interfaces;

public interface ISalesReportService
{
    Task<DailySalesReportDto> GetDailySalesReportAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        CancellationToken cancellationToken = default);

    Task<SalesSummaryReportDto> GetSalesSummaryReportAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        CancellationToken cancellationToken = default);

    Task<ItemWiseSalesReportDto> GetItemWiseSalesReportAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        string? itemCode,
        int? categoryId,
        CancellationToken cancellationToken = default);

    Task<SalesReturnReportDto> GetSalesReturnReportAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        string? itemCode,
        CancellationToken cancellationToken = default);
}
