using PosApi.DTOs.Reports;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class SalesReportService : ISalesReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SalesReportService> _logger;

    public SalesReportService(IUnitOfWork unitOfWork, ILogger<SalesReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DailySalesReportDto> GetDailySalesReportAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        CancellationToken cancellationToken = default)
    {
        var (rangeStart, rangeEnd) = ToInclusiveRange(fromDate, toDate);

        var sales = await _unitOfWork.SalesReports.GetSalesForReportAsync(
            rangeStart, rangeEnd, branchCode, cashierCode, customerCode, itemCode: null, categoryId: null, cancellationToken);

        var returns = await _unitOfWork.SalesReports.GetSaleReturnsForReportAsync(
            rangeStart, rangeEnd, branchCode, cashierCode, customerCode, itemCode: null, categoryId: null, cancellationToken);

        var report = new DailySalesReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            BranchCode = branchCode,
            BranchName = await GetBranchNameAsync(branchCode, cancellationToken)
        };

        for (var day = fromDate; day <= toDate; day = day.AddDays(1))
        {
            var daySales = sales.Where(s => s.SaleDate.HasValue && DateOnly.FromDateTime(s.SaleDate.Value) == day).ToList();
            var dayReturns = returns.Where(r => r.ReturnDate.HasValue && DateOnly.FromDateTime(r.ReturnDate.Value) == day).ToList();

            var row = BuildDailyRow(day, daySales, dayReturns);
            report.Days.Add(row);
        }

        report.Total = AccumulateRows(report.Days);

        return report;
    }

    public async Task<SalesSummaryReportDto> GetSalesSummaryReportAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        CancellationToken cancellationToken = default)
    {
        var (rangeStart, rangeEnd) = ToInclusiveRange(fromDate, toDate);

        var sales = await _unitOfWork.SalesReports.GetSalesForReportAsync(
            rangeStart, rangeEnd, branchCode, cashierCode, customerCode, itemCode: null, categoryId: null, cancellationToken);

        var returns = await _unitOfWork.SalesReports.GetSaleReturnsForReportAsync(
            rangeStart, rangeEnd, branchCode, cashierCode, customerCode, itemCode: null, categoryId: null, cancellationToken);

        var grossSales = sales.Sum(s => s.Subtotal ?? 0);
        var discountTotal = sales.Sum(s => s.DiscountAmount ?? 0);
        var taxTotal = sales.Sum(s => s.TaxAmount ?? 0);
        var returnsTotal = returns.Sum(r => r.TotalReturnAmount ?? 0);

        return new SalesSummaryReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            BranchCode = branchCode,
            BranchName = await GetBranchNameAsync(branchCode, cancellationToken),
            TotalInvoices = sales.Count,
            TotalQuantitySold = sales.SelectMany(s => s.Items).Sum(i => i.Quantity ?? 0),
            GrossSales = grossSales,
            DiscountTotal = discountTotal,
            TaxTotal = taxTotal,
            ReturnsTotal = returnsTotal,
            NetSales = grossSales - discountTotal + taxTotal - returnsTotal
        };
    }

    public async Task<ItemWiseSalesReportDto> GetItemWiseSalesReportAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        string? itemCode,
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var (rangeStart, rangeEnd) = ToInclusiveRange(fromDate, toDate);

        var sales = await _unitOfWork.SalesReports.GetSalesForReportAsync(
            rangeStart, rangeEnd, branchCode, cashierCode, customerCode, itemCode, categoryId, cancellationToken);

        var returns = await _unitOfWork.SalesReports.GetSaleReturnsForReportAsync(
            rangeStart, rangeEnd, branchCode, cashierCode, customerCode, itemCode, categoryId, cancellationToken);

        // Sales come back header-filtered (an invoice qualifies if ANY line matches), so the
        // individual lines still need to be narrowed down to the ones that actually match.
        var lines = sales.SelectMany(s => s.Items)
            .Where(i => itemCode is null || i.ItemCode == itemCode)
            .Where(i => categoryId is null || i.Product?.CategoryId == categoryId)
            .ToList();

        var returnLines = returns.SelectMany(r => r.Items)
            .Where(i => itemCode is null || i.ItemCode == itemCode)
            .Where(i => categoryId is null || i.Product?.CategoryId == categoryId)
            .ToList();

        var salesByItem = lines
            .GroupBy(i => i.ItemCode ?? string.Empty)
            .ToDictionary(g => g.Key, g => new
            {
                ItemName = g.Select(i => i.Product?.ItemName).FirstOrDefault(n => n is not null),
                Quantity = g.Sum(i => i.Quantity ?? 0),
                SellingAmount = g.Sum(i => (i.Quantity ?? 0) * (i.UnitPrice ?? 0)),
                DiscountAmount = g.Sum(i => i.DiscountAmount ?? 0)
            });

        var returnsByItem = returnLines
            .GroupBy(i => i.ItemCode ?? string.Empty)
            .ToDictionary(g => g.Key, g => new
            {
                Quantity = g.Sum(i => i.Quantity ?? 0),
                Amount = g.Sum(i => i.TotalAmount ?? 0)
            });

        var allItemCodes = salesByItem.Keys.Union(returnsByItem.Keys);

        var report = new ItemWiseSalesReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            BranchCode = branchCode,
            BranchName = await GetBranchNameAsync(branchCode, cancellationToken)
        };

        foreach (var code in allItemCodes.OrderBy(c => c))
        {
            salesByItem.TryGetValue(code, out var sold);
            returnsByItem.TryGetValue(code, out var returned);

            var sellingAmount = sold?.SellingAmount ?? 0;
            var discountAmount = sold?.DiscountAmount ?? 0;
            var returnAmount = returned?.Amount ?? 0;

            report.Items.Add(new ItemWiseSalesReportLineDto
            {
                ItemCode = code,
                ItemName = sold?.ItemName,
                QuantitySold = sold?.Quantity ?? 0,
                SellingAmount = sellingAmount,
                DiscountAmount = discountAmount,
                ReturnQuantity = returned?.Quantity ?? 0,
                ReturnAmount = returnAmount,
                NetSalesAmount = sellingAmount - discountAmount - returnAmount
            });
        }

        report.Total = new ItemWiseSalesReportLineDto
        {
            ItemCode = null,
            ItemName = "TOTAL",
            QuantitySold = report.Items.Sum(i => i.QuantitySold),
            SellingAmount = report.Items.Sum(i => i.SellingAmount),
            DiscountAmount = report.Items.Sum(i => i.DiscountAmount),
            ReturnQuantity = report.Items.Sum(i => i.ReturnQuantity),
            ReturnAmount = report.Items.Sum(i => i.ReturnAmount),
            NetSalesAmount = report.Items.Sum(i => i.NetSalesAmount)
        };

        return report;
    }

    public async Task<SalesReturnReportDto> GetSalesReturnReportAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        string? cashierCode,
        string? customerCode,
        string? itemCode,
        CancellationToken cancellationToken = default)
    {
        var (rangeStart, rangeEnd) = ToInclusiveRange(fromDate, toDate);

        var returns = await _unitOfWork.SalesReports.GetSaleReturnsForReportAsync(
            rangeStart, rangeEnd, branchCode, cashierCode, customerCode, itemCode, categoryId: null, cancellationToken);

        var report = new SalesReturnReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            BranchCode = branchCode,
            BranchName = await GetBranchNameAsync(branchCode, cancellationToken),
            TotalReturns = returns.Count,
            TotalReturnAmount = returns.Sum(r => r.TotalReturnAmount ?? 0)
        };

        foreach (var saleReturn in returns)
        {
            report.Returns.Add(new SalesReturnReportLineDto
            {
                ReturnNo = saleReturn.ReturnNo,
                ReturnDate = saleReturn.ReturnDate,
                InvoiceNo = saleReturn.InvoiceNo,
                BranchCode = saleReturn.Sale?.BranchCode,
                CustomerCode = saleReturn.Sale?.CustomerCode,
                CustomerName = saleReturn.Sale?.Customer?.CustomerName,
                Reason = saleReturn.Reason,
                CreatedBy = saleReturn.CreatedBy,
                CreatedByName = saleReturn.CreatedByUser?.FullName,
                TotalReturnAmount = saleReturn.TotalReturnAmount ?? 0,
                Items = saleReturn.Items.Select(i => new SalesReturnReportItemDto
                {
                    ItemCode = i.ItemCode,
                    ItemName = i.Product?.ItemName,
                    Quantity = i.Quantity ?? 0,
                    UnitPrice = i.UnitPrice ?? 0,
                    TotalAmount = i.TotalAmount ?? 0
                }).ToList()
            });
        }

        return report;
    }

    private static DailySalesReportRowDto BuildDailyRow(DateOnly day, List<Sale> daySales, List<SaleReturn> dayReturns)
    {
        var grossSales = daySales.Sum(s => s.Subtotal ?? 0);
        var discountTotal = daySales.Sum(s => s.DiscountAmount ?? 0);
        var taxTotal = daySales.Sum(s => s.TaxAmount ?? 0);
        var returnsTotal = dayReturns.Sum(r => r.TotalReturnAmount ?? 0);

        var paymentSummary = daySales
            .SelectMany(s => s.Payments)
            .Where(p => p.Status == PaymentStatus.Completed)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodSummaryDto
            {
                PaymentMethod = g.Key,
                Count = g.Count(),
                Amount = g.Sum(p => p.Amount ?? 0)
            })
            .OrderBy(p => p.PaymentMethod)
            .ToList();

        return new DailySalesReportRowDto
        {
            Date = day,
            InvoiceCount = daySales.Count,
            GrossSales = grossSales,
            DiscountTotal = discountTotal,
            TaxTotal = taxTotal,
            ReturnsTotal = returnsTotal,
            NetSales = grossSales - discountTotal + taxTotal - returnsTotal,
            PaymentSummary = paymentSummary
        };
    }

    private static DailySalesReportRowDto AccumulateRows(List<DailySalesReportRowDto> rows)
    {
        var total = new DailySalesReportRowDto
        {
            InvoiceCount = rows.Sum(r => r.InvoiceCount),
            GrossSales = rows.Sum(r => r.GrossSales),
            DiscountTotal = rows.Sum(r => r.DiscountTotal),
            TaxTotal = rows.Sum(r => r.TaxTotal),
            ReturnsTotal = rows.Sum(r => r.ReturnsTotal),
            NetSales = rows.Sum(r => r.NetSales)
        };

        total.PaymentSummary = rows
            .SelectMany(r => r.PaymentSummary)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodSummaryDto
            {
                PaymentMethod = g.Key,
                Count = g.Sum(p => p.Count),
                Amount = g.Sum(p => p.Amount)
            })
            .OrderBy(p => p.PaymentMethod)
            .ToList();

        return total;
    }

    private async Task<string?> GetBranchNameAsync(string? branchCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(branchCode))
        {
            return null;
        }

        var branch = await _unitOfWork.Branches.GetByIdAsync(branchCode, cancellationToken);
        return branch?.BranchName;
    }

    /// <summary>Converts an inclusive [fromDate, toDate] DateOnly range into the DateTime bounds used to query SaleDate/ReturnDate.</summary>
    private static (DateTime Start, DateTime End) ToInclusiveRange(DateOnly fromDate, DateOnly toDate)
    {
        var start = fromDate.ToDateTime(TimeOnly.MinValue);
        var end = toDate.ToDateTime(TimeOnly.MaxValue);
        return (start, end);
    }
}
