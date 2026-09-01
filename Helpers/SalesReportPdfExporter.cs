using System.Globalization;
using PosApi.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PosApi.Helpers;

/// <summary>
/// Renders Sales Report DTOs as downloadable/printable PDFs via QuestPDF. Each report type gets
/// its own render method but they all share the same header/footer/table styling helpers below.
/// </summary>
public static class SalesReportPdfExporter
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static byte[] ExportDailySalesReport(DailySalesReportDto report)
    {
        return BuildDocument("Daily Sales Report", report.FromDate, report.ToDate, report.BranchName, column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1.4f);
                    c.RelativeColumn(1f);
                    c.RelativeColumn(1.3f);
                    c.RelativeColumn(1.3f);
                    c.RelativeColumn(1f);
                    c.RelativeColumn(1.3f);
                    c.RelativeColumn(1.3f);
                });

                table.Header(header =>
                {
                    HeaderCell(header, "Date");
                    HeaderCell(header, "Invoices");
                    HeaderCell(header, "Gross Sales");
                    HeaderCell(header, "Discounts");
                    HeaderCell(header, "Tax");
                    HeaderCell(header, "Returns");
                    HeaderCell(header, "Net Sales");
                });

                foreach (var day in report.Days)
                {
                    Cell(table, day.Date.ToString("yyyy-MM-dd", Culture));
                    Cell(table, day.InvoiceCount.ToString(Culture));
                    Cell(table, Money(day.GrossSales));
                    Cell(table, Money(day.DiscountTotal));
                    Cell(table, Money(day.TaxTotal));
                    Cell(table, Money(day.ReturnsTotal));
                    Cell(table, Money(day.NetSales));
                }

                Cell(table, "TOTAL", bold: true);
                Cell(table, report.Total.InvoiceCount.ToString(Culture), bold: true);
                Cell(table, Money(report.Total.GrossSales), bold: true);
                Cell(table, Money(report.Total.DiscountTotal), bold: true);
                Cell(table, Money(report.Total.TaxTotal), bold: true);
                Cell(table, Money(report.Total.ReturnsTotal), bold: true);
                Cell(table, Money(report.Total.NetSales), bold: true);
            });

            column.Item().PaddingTop(14).Text("Payment Summary").Bold().FontSize(11);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2f);
                    c.RelativeColumn(1f);
                    c.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    HeaderCell(header, "Payment Method");
                    HeaderCell(header, "Count");
                    HeaderCell(header, "Amount");
                });

                foreach (var payment in report.Total.PaymentSummary)
                {
                    Cell(table, payment.PaymentMethod.ToString());
                    Cell(table, payment.Count.ToString(Culture));
                    Cell(table, Money(payment.Amount));
                }

                if (report.Total.PaymentSummary.Count == 0)
                {
                    Cell(table, "No payments recorded for this period.");
                    Cell(table, "");
                    Cell(table, "");
                }
            });
        });
    }

    public static byte[] ExportSalesSummaryReport(SalesSummaryReportDto report)
    {
        return BuildDocument("Sales Summary Report", report.FromDate, report.ToDate, report.BranchName, column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2f);
                    c.RelativeColumn(1.5f);
                });

                SummaryRow(table, "Total Invoices", report.TotalInvoices.ToString(Culture));
                SummaryRow(table, "Total Quantity Sold", report.TotalQuantitySold.ToString("N2", Culture));
                SummaryRow(table, "Gross Sales", Money(report.GrossSales));
                SummaryRow(table, "Discounts", Money(report.DiscountTotal));
                SummaryRow(table, "Tax", Money(report.TaxTotal));
                SummaryRow(table, "Returns", Money(report.ReturnsTotal));
                SummaryRow(table, "Net Sales", Money(report.NetSales), bold: true);
            });
        });
    }

    public static byte[] ExportItemWiseSalesReport(ItemWiseSalesReportDto report)
    {
        return BuildDocument("Item-wise Sales Report", report.FromDate, report.ToDate, report.BranchName, column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1.1f);
                    c.RelativeColumn(1.6f);
                    c.RelativeColumn(1f);
                    c.RelativeColumn(1.3f);
                    c.RelativeColumn(1.2f);
                    c.RelativeColumn(1f);
                    c.RelativeColumn(1.2f);
                    c.RelativeColumn(1.3f);
                });

                table.Header(header =>
                {
                    HeaderCell(header, "Item Code");
                    HeaderCell(header, "Item Name");
                    HeaderCell(header, "Qty Sold");
                    HeaderCell(header, "Selling Amt");
                    HeaderCell(header, "Discount");
                    HeaderCell(header, "Ret. Qty");
                    HeaderCell(header, "Ret. Amt");
                    HeaderCell(header, "Net Sales");
                });

                foreach (var line in report.Items)
                {
                    Cell(table, line.ItemCode ?? "-");
                    Cell(table, line.ItemName ?? "-");
                    Cell(table, line.QuantitySold.ToString("N2", Culture));
                    Cell(table, Money(line.SellingAmount));
                    Cell(table, Money(line.DiscountAmount));
                    Cell(table, line.ReturnQuantity.ToString("N2", Culture));
                    Cell(table, Money(line.ReturnAmount));
                    Cell(table, Money(line.NetSalesAmount));
                }

                Cell(table, "TOTAL", bold: true);
                Cell(table, "", bold: true);
                Cell(table, report.Total.QuantitySold.ToString("N2", Culture), bold: true);
                Cell(table, Money(report.Total.SellingAmount), bold: true);
                Cell(table, Money(report.Total.DiscountAmount), bold: true);
                Cell(table, report.Total.ReturnQuantity.ToString("N2", Culture), bold: true);
                Cell(table, Money(report.Total.ReturnAmount), bold: true);
                Cell(table, Money(report.Total.NetSalesAmount), bold: true);
            });
        });
    }

    public static byte[] ExportSalesReturnReport(SalesReturnReportDto report)
    {
        return BuildDocument("Sales Return Report", report.FromDate, report.ToDate, report.BranchName, column =>
        {
            column.Item().Text($"Total Returns: {report.TotalReturns}    Total Return Amount: {Money(report.TotalReturnAmount)}").Bold();

            foreach (var ret in report.Returns)
            {
                column.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(6).Column(inner =>
                {
                    inner.Item().Text($"Return No: {ret.ReturnNo}    Date: {ret.ReturnDate:yyyy-MM-dd HH:mm}    Invoice: {ret.InvoiceNo}").Bold();
                    inner.Item().Text($"Branch: {ret.BranchCode ?? "-"}    Customer: {ret.CustomerName ?? ret.CustomerCode ?? "Walk-in"}    Reason: {ret.Reason ?? "-"}");
                    inner.Item().Text($"Processed by: {ret.CreatedByName ?? ret.CreatedBy ?? "-"}    Return Amount: {Money(ret.TotalReturnAmount)}");

                    inner.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.2f);
                            c.RelativeColumn(2f);
                            c.RelativeColumn(1f);
                            c.RelativeColumn(1.2f);
                            c.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            HeaderCell(header, "Item Code");
                            HeaderCell(header, "Item Name");
                            HeaderCell(header, "Qty");
                            HeaderCell(header, "Unit Price");
                            HeaderCell(header, "Amount");
                        });

                        foreach (var item in ret.Items)
                        {
                            Cell(table, item.ItemCode ?? "-");
                            Cell(table, item.ItemName ?? "-");
                            Cell(table, item.Quantity.ToString("N2", Culture));
                            Cell(table, Money(item.UnitPrice));
                            Cell(table, Money(item.TotalAmount));
                        }
                    });
                });
            }

            if (report.Returns.Count == 0)
            {
                column.Item().PaddingTop(10).Text("No sales returns were recorded for this period.");
            }
        });
    }

    // -----------------------------------------------------------------------------------------
    // Shared layout helpers
    // -----------------------------------------------------------------------------------------

    private static byte[] BuildDocument(
        string title,
        DateOnly fromDate,
        DateOnly toDate,
        string? branchName,
        Action<ColumnDescriptor> content)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(header =>
                {
                    header.Item().Text(title).FontSize(16).Bold();
                    header.Item().Text($"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}" + (branchName is null ? " (All Branches)" : $" | Branch: {branchName}"));
                    header.Item().PaddingBottom(8).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                });

                page.Content().Column(content);

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8);
                    row.RelativeItem().AlignRight().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });
        }).GeneratePdf();
    }

    private static void HeaderCell(TableCellDescriptor header, string text)
    {
        header.Cell().Element(e => e.Background(Colors.Grey.Lighten2).Padding(4))
            .Text(text).Bold().FontSize(9);
    }

    private static void Cell(TableDescriptor table, string text, bool bold = false)
    {
        var container = table.Cell().Element(e => e.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4));
        if (bold)
        {
            container.Text(text).Bold();
        }
        else
        {
            container.Text(text);
        }
    }

    private static void SummaryRow(TableDescriptor table, string label, string value, bool bold = false)
    {
        var labelContainer = table.Cell().Element(e => e.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5));
        var valueContainer = table.Cell().Element(e => e.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight());

        if (bold)
        {
            labelContainer.Text(label).Bold();
            valueContainer.Text(value).Bold();
        }
        else
        {
            labelContainer.Text(label);
            valueContainer.Text(value);
        }
    }

    private static string Money(decimal amount) => amount.ToString("N2", Culture);
}
