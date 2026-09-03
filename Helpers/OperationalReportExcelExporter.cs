using ClosedXML.Excel;
using PosApi.DTOs.Reports;

namespace PosApi.Helpers;

public static class OperationalReportExcelExporter
{
    public static byte[] ExportCurrentStock(
        CurrentStockReportDto report)
    {
        var headers = new[]
        {
            "Stock ID",
            "Item Code",
            "Item Name",
            "Barcode",
            "Category",
            "Brand",
            "Branch",
            "Warehouse",
            "Available Qty",
            "Reorder Level",
            "Average Unit Cost",
            "Stock Value",
            "Below Reorder Level"
        };

        var rows = report.Items
            .Select(item => new object?[]
            {
                item.StockId,
                item.ItemCode,
                item.ItemName,
                item.Barcode,
                item.CategoryName,
                item.BrandName,
                item.BranchCode,
                item.WarehouseCode,
                item.AvailableQty,
                item.ReorderLevel,
                item.AverageUnitCost,
                item.StockValue,
                item.IsBelowReorderLevel ? "Yes" : "No"
            })
            .ToList();

        return GenerateWorkbook(
            "Current Stock",
            headers,
            rows,
            new Dictionary<string, object?>
            {
                ["Total Quantity"] =
                    report.TotalQuantity,

                ["Total Stock Value"] =
                    report.TotalStockValue
            });
    }

    public static byte[] ExportStockMovements(
        StockMovementReportDto report)
    {
        var headers = new[]
        {
            "Movement ID",
            "Date",
            "Item Code",
            "Item Name",
            "Branch",
            "Warehouse",
            "Batch",
            "Movement Type",
            "Reference Type",
            "Reference No",
            "Quantity",
            "Previous Qty",
            "New Qty",
            "Unit Cost",
            "Movement Value",
            "Created By",
            "Remarks"
        };

        var rows = report.Movements
            .Select(item => new object?[]
            {
                item.MovementId,
                item.CreatedAt,
                item.ItemCode,
                item.ItemName,
                item.BranchCode,
                item.WarehouseCode,
                item.BatchNo,
                item.MovementType.ToString(),
                item.ReferenceType.ToString(),
                item.ReferenceNo,
                item.Quantity,
                item.PreviousQty,
                item.NewQty,
                item.UnitCost,
                item.MovementValue,
                item.CreatedByName ?? item.CreatedBy,
                item.Remarks
            })
            .ToList();

        return GenerateWorkbook(
            "Stock Movements",
            headers,
            rows,
            new Dictionary<string, object?>
            {
                ["From Date"] = report.FromDate.ToString(),
                ["To Date"] = report.ToDate.ToString(),
                ["Total In Qty"] = report.TotalInQty,
                ["Total Out Qty"] = report.TotalOutQty,
                ["Total In Value"] = report.TotalInValue,
                ["Total Out Value"] = report.TotalOutValue
            });
    }

    public static byte[] ExportPurchases(
        PurchaseReportDto report)
    {
        var headers = new[]
        {
            "PO No",
            "PO Date",
            "Status",
            "Vendor ID",
            "Vendor",
            "Branch",
            "Item Code",
            "Item Name",
            "Ordered Qty",
            "Received Qty",
            "Outstanding Qty",
            "Returned Qty",
            "Net Received Qty",
            "Unit Cost",
            "Ordered Value",
            "Received Value",
            "Return Value",
            "Net Purchase Value"
        };

        var rows = report.Items
            .Select(item => new object?[]
            {
                item.PoNo,
                item.PoDate,
                item.PoStatus.ToString(),
                item.VendorId,
                item.VendorName,
                item.BranchCode,
                item.ItemCode,
                item.ItemName,
                item.OrderedQty,
                item.ReceivedQty,
                item.OutstandingQty,
                item.ReturnedQty,
                item.NetReceivedQty,
                item.UnitCost,
                item.OrderedValue,
                item.ReceivedValue,
                item.ReturnValue,
                item.NetPurchaseValue
            })
            .ToList();

        return GenerateWorkbook(
            "Purchases",
            headers,
            rows,
            new Dictionary<string, object?>
            {
                ["From Date"] = report.FromDate.ToString(),
                ["To Date"] = report.ToDate.ToString(),
                ["Total Ordered Value"] =
                    report.TotalOrderedValue,
                ["Total Received Value"] =
                    report.TotalReceivedValue,
                ["Total Return Value"] =
                    report.TotalReturnValue,
                ["Net Purchase Value"] =
                    report.TotalNetPurchaseValue
            });
    }

    public static byte[] ExportExpenses(
        ExpenseReportDto report)
    {
        var headers = new[]
        {
            "Expense ID",
            "Date",
            "Branch Code",
            "Branch Name",
            "Category ID",
            "Category",
            "Description",
            "Paid By",
            "Amount"
        };

        var rows = report.Expenses
            .Select(item => new object?[]
            {
                item.ExpenseId,
                item.ExpenseDate?.ToString(),
                item.BranchCode,
                item.BranchName,
                item.CategoryId,
                item.CategoryName,
                item.Description,
                item.PaidByName ?? item.PaidBy,
                item.Amount
            })
            .ToList();

        return GenerateWorkbook(
            "Expenses",
            headers,
            rows,
            new Dictionary<string, object?>
            {
                ["From Date"] = report.FromDate.ToString(),
                ["To Date"] = report.ToDate.ToString(),
                ["Expense Count"] =
                    report.TotalExpenseCount,
                ["Total Expense"] =
                    report.TotalExpenseAmount
            });
    }

    public static byte[] ExportProfit(
        ProfitReportDto report)
    {
        var headers = new[]
        {
            "Description",
            "Amount"
        };

        var rows = new List<object?[]>
        {
            new object?[]
            {
                "Gross Sales Excluding Tax",
                report.GrossSalesExcludingTax
            },
            new object?[]
            {
                "Discount Total",
                report.DiscountTotal
            },
            new object?[]
            {
                "Sales Return Total",
                report.SalesReturnTotal
            },
            new object?[]
            {
                "Net Revenue",
                report.NetRevenue
            },
            new object?[]
            {
                "Sold Cost",
                report.SoldCost
            },
            new object?[]
            {
                "Returned Cost",
                report.ReturnedCost
            },
            new object?[]
            {
                "Net Cost of Goods Sold",
                report.NetCostOfGoodsSold
            },
            new object?[]
            {
                "Gross Profit",
                report.GrossProfit
            },
            new object?[]
            {
                "Expense Total",
                report.ExpenseTotal
            },
            new object?[]
            {
                "Net Profit",
                report.NetProfit
            },
            new object?[]
            {
                "Gross Profit Margin Percentage",
                report.GrossProfitMarginPercentage
            }
        };

        return GenerateWorkbook(
            "Profit",
            headers,
            rows,
            new Dictionary<string, object?>
            {
                ["From Date"] = report.FromDate.ToString(),
                ["To Date"] = report.ToDate.ToString(),
                ["Net Profit"] = report.NetProfit
            });
    }

    private static byte[] GenerateWorkbook(
        string sheetName,
        IReadOnlyList<string> headers,
        IReadOnlyList<object?[]> rows,
        IReadOnlyDictionary<string, object?> summaries)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add(
            sheetName.Length > 31
                ? sheetName[..31]
                : sheetName);

        var rowNumber = 1;

        worksheet.Cell(rowNumber, 1).Value =
            $"{sheetName} Report";

        worksheet.Range(
                rowNumber,
                1,
                rowNumber,
                headers.Count)
            .Merge();

        worksheet.Cell(rowNumber, 1)
            .Style.Font.Bold = true;

        worksheet.Cell(rowNumber, 1)
            .Style.Font.FontSize = 16;

        worksheet.Cell(rowNumber, 1)
            .Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

        rowNumber += 2;

        foreach (var summary in summaries)
        {
            worksheet.Cell(rowNumber, 1).Value =
                summary.Key;

            worksheet.Cell(rowNumber, 2).Value =
                XLCellValue.FromObject(summary.Value);

            worksheet.Cell(rowNumber, 1)
                .Style.Font.Bold = true;

            rowNumber++;
        }

        rowNumber++;

        var headerRow = rowNumber;

        for (var column = 0;
             column < headers.Count;
             column++)
        {
            worksheet.Cell(
                headerRow,
                column + 1).Value = headers[column];
        }

        var headerRange = worksheet.Range(
            headerRow,
            1,
            headerRow,
            headers.Count);

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor =
            XLColor.DarkBlue;
        headerRange.Style.Font.FontColor =
            XLColor.White;
        headerRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        rowNumber++;

        foreach (var row in rows)
        {
            for (var column = 0;
                 column < headers.Count;
                 column++)
            {
                var value = column < row.Length
                    ? row[column]
                    : null;

                worksheet.Cell(
                    rowNumber,
                    column + 1).Value =
                        XLCellValue.FromObject(value);
            }

            rowNumber++;
        }

        if (rows.Count > 0)
        {
            var dataRange = worksheet.Range(
                headerRow,
                1,
                rowNumber - 1,
                headers.Count);

            dataRange.CreateTable();
        }

        worksheet.SheetView.FreezeRows(headerRow);

        worksheet.Columns().AdjustToContents();

        foreach (var column in worksheet.ColumnsUsed())
        {
            if (column.Width > 40)
            {
                column.Width = 40;
            }
        }

        worksheet.Rows().Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Style.Font.FontName = "Arial";
        worksheet.Style.Font.FontSize = 10;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return stream.ToArray();
    }
}