using PosApi.DTOs.Reports;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class OperationalReportService
    : IOperationalReportService
{
    private readonly IOperationalReportRepository _repository;

    public OperationalReportService(
        IOperationalReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<CurrentStockReportDto> GetCurrentStockAsync(
        string? branchCode,
        string? warehouseCode,
        string? itemCode,
        int? categoryId,
        bool onlyAvailable,
        bool onlyBelowReorderLevel,
        CancellationToken cancellationToken = default)
    {
        var stocks = await _repository.GetCurrentStockAsync(
            branchCode,
            warehouseCode,
            itemCode,
            categoryId,
            onlyAvailable,
            onlyBelowReorderLevel,
            cancellationToken);

        var report = new CurrentStockReportDto
        {
            GeneratedAt = DateTime.UtcNow,
            BranchCode = branchCode
        };

        foreach (var stock in stocks)
        {
            var stockValue = stock.Batches.Sum(
                batch => batch.AvailableQty * batch.UnitCost);

            var averageCost = stock.CurrentQty > 0
                ? stockValue / stock.CurrentQty
                : 0;

            report.Items.Add(new CurrentStockReportLineDto
            {
                StockId = stock.StockId,
                ItemCode = stock.ItemCode,
                ItemName = stock.Product?.ItemName ?? stock.ItemCode,
                Barcode = stock.Product?.Barcode,
                CategoryName = stock.Product?.Category?.CategoryName,
                BrandName = stock.Product?.Brand?.BrandName,
                BranchCode = stock.BranchCode,
                WarehouseCode = stock.WarehouseCode,
                AvailableQty = stock.CurrentQty,
                ReorderLevel = stock.Product?.ReorderLevel,
                AverageUnitCost = Math.Round(averageCost, 2),
                StockValue = Math.Round(stockValue, 2),
                IsBelowReorderLevel =
                    stock.Product?.ReorderLevel.HasValue == true &&
                    stock.CurrentQty <= stock.Product.ReorderLevel.Value
            });
        }

        report.TotalQuantity =
            report.Items.Sum(x => x.AvailableQty);

        report.TotalStockValue =
            report.Items.Sum(x => x.StockValue);

        return report;
    }

    public async Task<StockMovementReportDto>
        GetStockMovementsAsync(
            DateOnly fromDate,
            DateOnly toDate,
            string? branchCode,
            string? warehouseCode,
            string? itemCode,
            StockMovementType? movementType,
            StockReferenceType? referenceType,
            string? referenceNo,
            CancellationToken cancellationToken = default)
    {
        var (start, end) = ConvertRange(fromDate, toDate);

        var movements = await _repository.GetStockMovementsAsync(
            start,
            end,
            branchCode,
            warehouseCode,
            itemCode,
            movementType,
            referenceType,
            referenceNo,
            cancellationToken);

        var report = new StockMovementReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            BranchCode = branchCode
        };

        report.Movements = movements.Select(x =>
            new StockMovementReportLineDto
            {
                MovementId = x.MovementId,
                CreatedAt = x.CreatedAt,
                ItemCode =
                    x.StockInventory?.ItemCode ?? string.Empty,
                ItemName =
                    x.StockInventory?.Product?.ItemName,
                BranchCode =
                    x.StockInventory?.BranchCode ?? string.Empty,
                WarehouseCode =
                    x.StockInventory?.WarehouseCode ?? string.Empty,
                BatchId = x.BatchId,
                BatchNo = x.StockBatch?.BatchNo,
                MovementType = x.MovementType,
                ReferenceType = x.ReferenceType,
                ReferenceNo = x.ReferenceNo,
                Quantity = x.Qty,
                PreviousQty = x.PreviousQty,
                NewQty = x.NewQty,
                UnitCost = x.UnitCost,
                MovementValue =
                    Math.Abs(x.Qty) * x.UnitCost,
                CreatedBy = x.CreatedBy,
                CreatedByName =
                    x.CreatedByUser?.FullName,
                Remarks = x.Remarks
            }).ToList();

        report.TotalInQty = movements
            .Where(x => x.Qty > 0)
            .Sum(x => x.Qty);

        report.TotalOutQty = movements
            .Where(x => x.Qty < 0)
            .Sum(x => Math.Abs(x.Qty));

        report.TotalInValue = movements
            .Where(x => x.Qty > 0)
            .Sum(x => x.Qty * x.UnitCost);

        report.TotalOutValue = movements
            .Where(x => x.Qty < 0)
            .Sum(x => Math.Abs(x.Qty) * x.UnitCost);

        return report;
    }

    public async Task<PurchaseReportDto> GetPurchasesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        int? vendorId,
        string? itemCode,
        PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = ConvertRange(fromDate, toDate);

        var orders = await _repository.GetPurchaseOrdersAsync(
            start,
            end,
            branchCode,
            vendorId,
            itemCode,
            status,
            cancellationToken);

        var grns = await _repository.GetGrnsAsync(
            branchCode,
            vendorId,
            itemCode,
            cancellationToken);

        var returns = await _repository.GetGrnReturnsAsync(
            branchCode,
            vendorId,
            itemCode,
            cancellationToken);

        var report = new PurchaseReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            BranchCode = branchCode
        };

        foreach (var order in orders)
        {
            foreach (var line in order.Items)
            {
                if (!string.IsNullOrWhiteSpace(itemCode) &&
                    line.ItemCode != itemCode)
                {
                    continue;
                }

                var grnLines = grns
                    .Where(g => g.PoNo == order.PoNo)
                    .SelectMany(g => g.Items)
                    .Where(g => g.ItemCode == line.ItemCode)
                    .ToList();

                var returnLines = returns
                    .Where(r => r.GrnMaster?.PoNo == order.PoNo)
                    .SelectMany(r => r.Items)
                    .Where(r => r.ItemCode == line.ItemCode)
                    .ToList();

                var orderedQty = line.Quantity ?? 0;
                var receivedQty =
                    grnLines.Sum(x => x.Quantity ?? 0);
                var returnedQty =
                    returnLines.Sum(x => x.Quantity ?? 0);

                var orderedValue =
                    orderedQty * (line.UnitCost ?? 0);

                var receivedValue =
                    grnLines.Sum(x => x.TotalCost ?? 0);

                var returnValue =
                    returnLines.Sum(x => x.TotalAmount ?? 0);

                report.Items.Add(new PurchaseReportLineDto
                {
                    PoNo = order.PoNo,
                    PoDate = order.PoDate,
                    PoStatus = order.Status,
                    VendorId = order.VendorId,
                    VendorName = order.Vendor?.VendorName,
                    BranchCode = order.BranchCode,
                    ItemCode = line.ItemCode ?? string.Empty,
                    ItemName = line.Product?.ItemName,
                    OrderedQty = orderedQty,
                    ReceivedQty = receivedQty,
                    OutstandingQty =
                        Math.Max(orderedQty - receivedQty, 0),
                    ReturnedQty = returnedQty,
                    NetReceivedQty =
                        receivedQty - returnedQty,
                    UnitCost = line.UnitCost ?? 0,
                    OrderedValue = orderedValue,
                    ReceivedValue = receivedValue,
                    ReturnValue = returnValue,
                    NetPurchaseValue =
                        receivedValue - returnValue
                });
            }
        }

        report.TotalOrderedValue =
            report.Items.Sum(x => x.OrderedValue);

        report.TotalReceivedValue =
            report.Items.Sum(x => x.ReceivedValue);

        report.TotalReturnValue =
            report.Items.Sum(x => x.ReturnValue);

        report.TotalNetPurchaseValue =
            report.Items.Sum(x => x.NetPurchaseValue);

        return report;
    }

    public async Task<ExpenseReportDto> GetExpensesAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        int? categoryId,
        string? paidBy,
        CancellationToken cancellationToken = default)
    {
        var expenses = await _repository.GetExpensesAsync(
            fromDate,
            toDate,
            branchCode,
            categoryId,
            paidBy,
            cancellationToken);

        var report = new ExpenseReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            BranchCode = branchCode
        };

        report.Expenses = expenses.Select(x =>
            new ExpenseReportLineDto
            {
                ExpenseId = x.ExpenseId,
                ExpenseDate = x.ExpenseDate,
                BranchCode = x.BranchCode,
                BranchName = x.Branch?.BranchName,
                CategoryId = x.CategoryId,
                CategoryName = x.Category?.CategoryName,
                Amount = x.Amount ?? 0,
                Description = x.Description,
                PaidBy = x.PaidBy,
                PaidByName = x.PaidByUser?.FullName
            }).ToList();

        report.CategorySummary = expenses
            .GroupBy(x => new
            {
                x.CategoryId,
                Name = x.Category?.CategoryName
            })
            .Select(group => new ExpenseCategorySummaryDto
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.Name,
                ExpenseCount = group.Count(),
                TotalAmount =
                    group.Sum(x => x.Amount ?? 0)
            })
            .OrderBy(x => x.CategoryName)
            .ToList();

        report.TotalExpenseCount =
            report.Expenses.Count;

        report.TotalExpenseAmount =
            report.Expenses.Sum(x => x.Amount);

        return report;
    }

    public async Task<ProfitReportDto> GetProfitAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? branchCode,
        CancellationToken cancellationToken = default)
    {
        var (start, end) = ConvertRange(fromDate, toDate);

        var sales = await _repository.GetSalesAsync(
            start,
            end,
            branchCode,
            cancellationToken);

        var returns = await _repository.GetSaleReturnsAsync(
            start,
            end,
            branchCode,
            cancellationToken);

        var movements =
            await _repository.GetProfitMovementsAsync(
                start,
                end,
                branchCode,
                cancellationToken);

        var expenses = await _repository.GetExpensesAsync(
            fromDate,
            toDate,
            branchCode,
            null,
            null,
            cancellationToken);

        var grossSales = sales
            .SelectMany(x => x.Items)
            .Sum(x =>
                (x.Quantity ?? 0) *
                (x.UnitPrice ?? 0));

        var discountTotal =
            sales.Sum(x => x.DiscountAmount ?? 0);

        var returnTotal =
            returns.Sum(x => x.TotalReturnAmount ?? 0);

        var netRevenue =
            grossSales - discountTotal - returnTotal;

        var soldCost = movements
            .Where(x =>
                x.ReferenceType == StockReferenceType.Sale &&
                x.MovementType == StockMovementType.Out)
            .Sum(x =>
                Math.Abs(x.Qty) * x.UnitCost);

        var returnedCost = movements
            .Where(x =>
                x.ReferenceType == StockReferenceType.SaleReturn &&
                x.MovementType == StockMovementType.In)
            .Sum(x =>
                x.Qty * x.UnitCost);

        var netCost = soldCost - returnedCost;
        var grossProfit = netRevenue - netCost;
        var expenseTotal =
            expenses.Sum(x => x.Amount ?? 0);
        var netProfit = grossProfit - expenseTotal;

        return new ProfitReportDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            BranchCode = branchCode,
            GrossSalesExcludingTax = grossSales,
            DiscountTotal = discountTotal,
            SalesReturnTotal = returnTotal,
            NetRevenue = netRevenue,
            SoldCost = soldCost,
            ReturnedCost = returnedCost,
            NetCostOfGoodsSold = netCost,
            GrossProfit = grossProfit,
            ExpenseTotal = expenseTotal,
            NetProfit = netProfit,
            GrossProfitMarginPercentage =
                netRevenue == 0
                    ? 0
                    : Math.Round(
                        grossProfit / netRevenue * 100,
                        2)
        };
    }

    private static (DateTime Start, DateTime End)
        ConvertRange(
            DateOnly fromDate,
            DateOnly toDate)
    {
        return (
            fromDate.ToDateTime(TimeOnly.MinValue),
            toDate.ToDateTime(TimeOnly.MaxValue)
        );
    }
}