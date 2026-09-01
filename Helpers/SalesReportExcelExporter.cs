using ClosedXML.Excel;
using PosApi.DTOs.Reports;

namespace PosApi.Helpers;

/// <summary>
/// Renders Sales Report DTOs as downloadable .xlsx workbooks via ClosedXML. Each method builds a
/// single-sheet workbook and returns the raw file bytes ready to hand back as a FileContentResult.
/// </summary>
public static class SalesReportExcelExporter
{
    private const string HeaderFill = "D9D9D9";

    public static byte[] ExportDailySalesReport(DailySalesReportDto report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Daily Sales");

        WriteTitle(sheet, "Daily Sales Report", report.FromDate, report.ToDate, report.BranchName);

        var row = 4;
        var headers = new[] { "Date", "Invoices", "Gross Sales", "Discounts", "Tax", "Returns", "Net Sales" };
        WriteHeaderRow(sheet, row, headers);
        row++;

        foreach (var day in report.Days)
        {
            sheet.Cell(row, 1).Value = day.Date.ToString("yyyy-MM-dd");
            sheet.Cell(row, 2).Value = day.InvoiceCount;
            sheet.Cell(row, 3).Value = day.GrossSales;
            sheet.Cell(row, 4).Value = day.DiscountTotal;
            sheet.Cell(row, 5).Value = day.TaxTotal;
            sheet.Cell(row, 6).Value = day.ReturnsTotal;
            sheet.Cell(row, 7).Value = day.NetSales;
            row++;
        }

        var totalRow = row;
        sheet.Cell(totalRow, 1).Value = "TOTAL";
        sheet.Cell(totalRow, 2).Value = report.Total.InvoiceCount;
        sheet.Cell(totalRow, 3).Value = report.Total.GrossSales;
        sheet.Cell(totalRow, 4).Value = report.Total.DiscountTotal;
        sheet.Cell(totalRow, 5).Value = report.Total.TaxTotal;
        sheet.Cell(totalRow, 6).Value = report.Total.ReturnsTotal;
        sheet.Cell(totalRow, 7).Value = report.Total.NetSales;
        sheet.Range(totalRow, 1, totalRow, 7).Style.Font.SetBold();

        FormatMoneyColumns(sheet, 4, row, new[] { 3, 4, 5, 6, 7 });

        // Payment Summary header section (in ExportDailySalesReport)
        row += 2;
        sheet.Cell(row, 1).Value = "Payment Summary";
        var paymentSummaryFont = sheet.Cell(row, 1).Style.Font;
        paymentSummaryFont.SetBold();
        paymentSummaryFont.FontSize = 12;
        row++;

        WriteHeaderRow(sheet, row, new[] { "Payment Method", "Count", "Amount" });
        row++;
        var paymentStart = row;
        foreach (var payment in report.Total.PaymentSummary)
        {
            sheet.Cell(row, 1).Value = payment.PaymentMethod.ToString();
            sheet.Cell(row, 2).Value = payment.Count;
            sheet.Cell(row, 3).Value = payment.Amount;
            row++;
        }
        FormatMoneyColumns(sheet, paymentStart, row - 1, new[] { 3 });

        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    public static byte[] ExportSalesSummaryReport(SalesSummaryReportDto report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Sales Summary");

        WriteTitle(sheet, "Sales Summary Report", report.FromDate, report.ToDate, report.BranchName);

        var row = 4;
        void IntLine(string label, int value, bool bold = false)
        {
            sheet.Cell(row, 1).Value = label;
            sheet.Cell(row, 2).Value = value;
            if (bold)
            {
                sheet.Range(row, 1, row, 2).Style.Font.SetBold();
            }
            row++;
        }

        void DecimalLine(string label, decimal value, bool bold = false)
        {
            sheet.Cell(row, 1).Value = label;
            sheet.Cell(row, 2).Value = value;
            if (bold)
            {
                sheet.Range(row, 1, row, 2).Style.Font.SetBold();
            }
            row++;
        }

        IntLine("Total Invoices", report.TotalInvoices);
        DecimalLine("Total Quantity Sold", report.TotalQuantitySold);
        DecimalLine("Gross Sales", report.GrossSales);
        DecimalLine("Discounts", report.DiscountTotal);
        DecimalLine("Tax", report.TaxTotal);
        DecimalLine("Returns", report.ReturnsTotal);
        DecimalLine("Net Sales", report.NetSales, bold: true);

        sheet.Range(4, 2, row - 1, 2).Style.NumberFormat.Format = "#,##0.00";
        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    public static byte[] ExportItemWiseSalesReport(ItemWiseSalesReportDto report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Item-wise Sales");

        WriteTitle(sheet, "Item-wise Sales Report", report.FromDate, report.ToDate, report.BranchName);

        var row = 4;
        WriteHeaderRow(sheet, row, new[] { "Item Code", "Item Name", "Qty Sold", "Selling Amount", "Discount", "Return Qty", "Return Amount", "Net Sales Amount" });
        row++;

        foreach (var line in report.Items)
        {
            sheet.Cell(row, 1).Value = line.ItemCode;
            sheet.Cell(row, 2).Value = line.ItemName;
            sheet.Cell(row, 3).Value = line.QuantitySold;
            sheet.Cell(row, 4).Value = line.SellingAmount;
            sheet.Cell(row, 5).Value = line.DiscountAmount;
            sheet.Cell(row, 6).Value = line.ReturnQuantity;
            sheet.Cell(row, 7).Value = line.ReturnAmount;
            sheet.Cell(row, 8).Value = line.NetSalesAmount;
            row++;
        }

        sheet.Cell(row, 1).Value = "TOTAL";
        sheet.Cell(row, 3).Value = report.Total.QuantitySold;
        sheet.Cell(row, 4).Value = report.Total.SellingAmount;
        sheet.Cell(row, 5).Value = report.Total.DiscountAmount;
        sheet.Cell(row, 6).Value = report.Total.ReturnQuantity;
        sheet.Cell(row, 7).Value = report.Total.ReturnAmount;
        sheet.Cell(row, 8).Value = report.Total.NetSalesAmount;
        sheet.Range(row, 1, row, 8).Style.Font.SetBold();

        FormatMoneyColumns(sheet, 4, row, new[] { 4, 5, 7, 8 });

        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    public static byte[] ExportSalesReturnReport(SalesReturnReportDto report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Sales Returns");

        WriteTitle(sheet, "Sales Return Report", report.FromDate, report.ToDate, report.BranchName);

        sheet.Cell(4, 1).Value = "Total Returns";
        sheet.Cell(4, 2).Value = report.TotalReturns;
        sheet.Cell(5, 1).Value = "Total Return Amount";
        sheet.Cell(5, 2).Value = report.TotalReturnAmount;
        sheet.Cell(5, 2).Style.NumberFormat.Format = "#,##0.00";

        var row = 7;
        WriteHeaderRow(sheet, row, new[]
        {
            "Return No", "Return Date", "Invoice No", "Branch", "Customer", "Reason",
            "Processed By", "Item Code", "Item Name", "Qty", "Unit Price", "Line Amount", "Return Total"
        });
        row++;

        foreach (var ret in report.Returns)
        {
            if (ret.Items.Count == 0)
            {
                WriteReturnHeaderCells(sheet, row, ret);
                row++;
                continue;
            }

            foreach (var item in ret.Items)
            {
                WriteReturnHeaderCells(sheet, row, ret);
                sheet.Cell(row, 8).Value = item.ItemCode;
                sheet.Cell(row, 9).Value = item.ItemName;
                sheet.Cell(row, 10).Value = item.Quantity;
                sheet.Cell(row, 11).Value = item.UnitPrice;
                sheet.Cell(row, 12).Value = item.TotalAmount;
                row++;
            }
        }

        FormatMoneyColumns(sheet, 8, row - 1, new[] { 11, 12, 13 });

        sheet.Columns().AdjustToContents();
        return ToBytes(workbook);
    }

    private static void WriteReturnHeaderCells(IXLWorksheet sheet, int row, SalesReturnReportLineDto ret)
    {
        sheet.Cell(row, 1).Value = ret.ReturnNo;
        sheet.Cell(row, 2).Value = ret.ReturnDate?.ToString("yyyy-MM-dd HH:mm") ?? "-";
        sheet.Cell(row, 3).Value = ret.InvoiceNo;
        sheet.Cell(row, 4).Value = ret.BranchCode;
        sheet.Cell(row, 5).Value = ret.CustomerName ?? ret.CustomerCode ?? "Walk-in";
        sheet.Cell(row, 6).Value = ret.Reason;
        sheet.Cell(row, 7).Value = ret.CreatedByName ?? ret.CreatedBy;
        sheet.Cell(row, 13).Value = ret.TotalReturnAmount;
    }

    // WriteTitle method
    private static void WriteTitle(IXLWorksheet sheet, string title, DateOnly fromDate, DateOnly toDate, string? branchName)
    {
        var titleCell = sheet.Cell(1, 1);
        titleCell.Value = title;
        var titleFont = titleCell.Style.Font;
        titleFont.SetBold();
        titleFont.FontSize = 14;

        sheet.Cell(2, 1).Value = $"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}";
        sheet.Cell(2, 3).Value = branchName is null ? "All Branches" : $"Branch: {branchName}";
    }

    private static void WriteHeaderRow(IXLWorksheet sheet, int row, string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(row, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.SetBold();
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml($"#{HeaderFill}"));
        }
    }

    private static void FormatMoneyColumns(IXLWorksheet sheet, int fromRow, int toRow, int[] columns)
    {
        if (toRow < fromRow)
        {
            return;
        }

        foreach (var col in columns)
        {
            sheet.Range(fromRow, col, toRow, col).Style.NumberFormat.Format = "#,##0.00";
        }
    }

    private static byte[] ToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
