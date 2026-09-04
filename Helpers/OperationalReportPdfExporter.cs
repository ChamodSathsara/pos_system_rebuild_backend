using System.Globalization;
using PosApi.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PosApi.Helpers;

public static class OperationalReportPdfExporter
{
    private static readonly CultureInfo Culture =
        CultureInfo.InvariantCulture;

    public static byte[] ExportCurrentStock(
        CurrentStockReportDto report)
    {
        var rows = report.Items
            .Select(item => new[]
            {
                item.ItemCode,
                item.ItemName,
                item.BranchCode,
                item.WarehouseCode,
                FormatQty(item.AvailableQty),
                Money(item.AverageUnitCost),
                Money(item.StockValue),
                item.IsBelowReorderLevel ? "YES" : "NO"
            })
            .ToList();

        var summaries = new Dictionary<string, string>
        {
            ["Total Quantity"] =
                FormatQty(report.TotalQuantity),

            ["Total Stock Value"] =
                Money(report.TotalStockValue)
        };

        return GenerateReport(
            title: "Current Stock Report",
            subtitle:
                $"Generated: {report.GeneratedAt:dd/MM/yyyy HH:mm}",
            headers: new[]
            {
                "Item Code",
                "Item Name",
                "Branch",
                "Warehouse",
                "Qty",
                "Avg Cost",
                "Value",
                "Low"
            },
            rows,
            summaries);
    }

    public static byte[] ExportStockMovements(
        StockMovementReportDto report)
    {
        var rows = report.Movements
            .Select(item => new[]
            {
                item.CreatedAt.ToString(
                    "dd/MM/yyyy HH:mm",
                    Culture),

                item.ItemCode,
                item.ItemName ?? string.Empty,
                item.BranchCode,
                item.WarehouseCode,
                item.BatchNo ?? string.Empty,
                item.MovementType.ToString(),
                item.ReferenceType.ToString(),
                item.ReferenceNo ?? string.Empty,
                FormatQty(item.Quantity),
                Money(item.UnitCost),
                Money(item.MovementValue)
            })
            .ToList();

        var summaries = new Dictionary<string, string>
        {
            ["Total In Qty"] =
                FormatQty(report.TotalInQty),

            ["Total Out Qty"] =
                FormatQty(report.TotalOutQty),

            ["Total In Value"] =
                Money(report.TotalInValue),

            ["Total Out Value"] =
                Money(report.TotalOutValue)
        };

        return GenerateReport(
            title: "Stock Movement Report",
            subtitle:
                $"{report.FromDate:dd/MM/yyyy} - " +
                $"{report.ToDate:dd/MM/yyyy}",
            headers: new[]
            {
                "Date",
                "Code",
                "Item",
                "Branch",
                "Warehouse",
                "Batch",
                "Type",
                "Reference Type",
                "Reference",
                "Qty",
                "Unit Cost",
                "Value"
            },
            rows,
            summaries);
    }

    public static byte[] ExportPurchases(
        PurchaseReportDto report)
    {
        var rows = report.Items
            .Select(item => new[]
            {
                item.PoNo,
                item.PoDate?.ToString(
                    "dd/MM/yyyy",
                    Culture) ?? string.Empty,

                item.VendorName ?? string.Empty,
                item.ItemCode,
                item.ItemName ?? string.Empty,
                FormatQty(item.OrderedQty),
                FormatQty(item.ReceivedQty),
                FormatQty(item.OutstandingQty),
                FormatQty(item.ReturnedQty),
                FormatQty(item.NetReceivedQty),
                Money(item.OrderedValue),
                Money(item.NetPurchaseValue)
            })
            .ToList();

        var summaries = new Dictionary<string, string>
        {
            ["Total Ordered Value"] =
                Money(report.TotalOrderedValue),

            ["Total Received Value"] =
                Money(report.TotalReceivedValue),

            ["Total Return Value"] =
                Money(report.TotalReturnValue),

            ["Net Purchase Value"] =
                Money(report.TotalNetPurchaseValue)
        };

        return GenerateReport(
            title: "Purchase / GRN Report",
            subtitle:
                $"{report.FromDate:dd/MM/yyyy} - " +
                $"{report.ToDate:dd/MM/yyyy}",
            headers: new[]
            {
                "PO No",
                "PO Date",
                "Vendor",
                "Code",
                "Item",
                "Ordered",
                "Received",
                "Outstanding",
                "Returned",
                "Net Qty",
                "Ordered Value",
                "Net Value"
            },
            rows,
            summaries);
    }

    public static byte[] ExportExpenses(
        ExpenseReportDto report)
    {
        var rows = report.Expenses
            .Select(item => new[]
            {
                item.ExpenseDate?.ToString(
                    "dd/MM/yyyy",
                    Culture) ?? string.Empty,

                item.BranchCode ?? string.Empty,
                item.CategoryName ?? string.Empty,
                item.Description ?? string.Empty,
                item.PaidByName ??
                    item.PaidBy ??
                    string.Empty,

                Money(item.Amount)
            })
            .ToList();

        var summaries = new Dictionary<string, string>
        {
            ["Expense Count"] =
                report.TotalExpenseCount.ToString(Culture),

            ["Total Expense"] =
                Money(report.TotalExpenseAmount)
        };

        return GenerateReport(
            title: "Expense Report",
            subtitle:
                $"{report.FromDate:dd/MM/yyyy} - " +
                $"{report.ToDate:dd/MM/yyyy}",
            headers: new[]
            {
                "Date",
                "Branch",
                "Category",
                "Description",
                "Paid By",
                "Amount"
            },
            rows,
            summaries);
    }

    public static byte[] ExportProfit(
        ProfitReportDto report)
    {
        var rows = new List<string[]>
        {
            new[]
            {
                "Gross Sales (Excluding Tax)",
                Money(report.GrossSalesExcludingTax)
            },
            new[]
            {
                "Discount Total",
                Money(report.DiscountTotal)
            },
            new[]
            {
                "Sales Return Total",
                Money(report.SalesReturnTotal)
            },
            new[]
            {
                "Net Revenue",
                Money(report.NetRevenue)
            },
            new[]
            {
                "Sold Cost",
                Money(report.SoldCost)
            },
            new[]
            {
                "Returned Cost",
                Money(report.ReturnedCost)
            },
            new[]
            {
                "Net Cost of Goods Sold",
                Money(report.NetCostOfGoodsSold)
            },
            new[]
            {
                "Gross Profit",
                Money(report.GrossProfit)
            },
            new[]
            {
                "Expense Total",
                Money(report.ExpenseTotal)
            },
            new[]
            {
                "Net Profit",
                Money(report.NetProfit)
            },
            new[]
            {
                "Gross Profit Margin",
                $"{report.GrossProfitMarginPercentage:N2}%"
            }
        };

        var summaries = new Dictionary<string, string>
        {
            ["Net Profit"] = Money(report.NetProfit)
        };

        return GenerateReport(
            title: "Profit Report",
            subtitle:
                $"{report.FromDate:dd/MM/yyyy} - " +
                $"{report.ToDate:dd/MM/yyyy}",
            headers: new[]
            {
                "Description",
                "Amount"
            },
            rows,
            summaries);
    }

    public static byte[] ExportDamageItems(DamageItemReportDto report)
    {
        var rows = report.Items.Select(item => new[]
        {
            item.DamageDate?.ToString("dd/MM/yyyy", Culture) ?? string.Empty,
            item.ItemCode ?? string.Empty,
            item.ItemName ?? string.Empty,
            item.BranchCode ?? string.Empty,
            item.WarehouseCode ?? string.Empty,
            FormatQty(item.Quantity),
            Money(item.CostAmount),
            item.Reason ?? string.Empty,
            item.ReportedByName ?? item.ReportedBy ?? string.Empty,
            item.Status.ToString()
        }).ToList();

        return GenerateReport(
            "Damaged Items Report",
            $"{report.FromDate:dd/MM/yyyy} - {report.ToDate:dd/MM/yyyy}",
            new[] { "Date", "Code", "Item", "Branch", "Warehouse", "Qty", "Cost", "Reason", "Reported By", "Status" },
            rows,
            new Dictionary<string, string>
            {
                ["Damage Count"] = report.TotalDamageCount.ToString(Culture),
                ["Total Quantity"] = FormatQty(report.TotalQuantity),
                ["Total Damage Cost"] = Money(report.TotalDamageCost)
            });
    }

    private static byte[] GenerateReport(
        string title,
        string subtitle,
        IReadOnlyList<string> headers,
        IReadOnlyList<string[]> rows,
        IReadOnlyDictionary<string, string> summaries)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(
                    style => style.FontSize(8));

                page.Header().Column(column =>
                {
                    column.Item()
                        .Text(title)
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item()
                        .Text(subtitle)
                        .FontSize(10);

                    column.Item()
                        .PaddingBottom(8)
                        .Text(
                            $"Generated at: " +
                            $"{DateTime.Now:dd/MM/yyyy HH:mm}");
                });

                page.Content().Column(column =>
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in headers)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var heading in headers)
                            {
                                header.Cell()
                                    .Element(HeaderCell)
                                    .Text(heading)
                                    .Bold();
                            }
                        });

                        if (rows.Count == 0)
                        {
                            table.Cell()
                                .ColumnSpan((uint)headers.Count)
                                .Element(DataCell)
                                .AlignCenter()
                                .Text("No data available.");
                        }
                        else
                        {
                            foreach (var row in rows)
                            {
                                foreach (var value in row)
                                {
                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(value ?? string.Empty);
                                }
                            }
                        }
                    });

                    column.Item()
                        .PaddingTop(15)
                        .AlignRight()
                        .Width(250)
                        .Column(summaryColumn =>
                        {
                            foreach (var summary in summaries)
                            {
                                summaryColumn.Item()
                                    .PaddingVertical(2)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Text(summary.Key)
                                            .Bold();

                                        row.ConstantItem(100)
                                            .AlignRight()
                                            .Text(summary.Value)
                                            .Bold();
                                    });
                            }
                        });
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    private static IContainer HeaderCell(
        IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten2)
            .Border(0.5f)
            .BorderColor(Colors.Grey.Medium)
            .Padding(4);
    }

    private static IContainer DataCell(
        IContainer container)
    {
        return container
            .BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten1)
            .Padding(3);
    }

    private static string Money(decimal value)
    {
        return value.ToString("N2", Culture);
    }

    private static string FormatQty(decimal value)
    {
        return value == Math.Truncate(value)
            ? value.ToString("N0", Culture)
            : value.ToString("N3", Culture);
    }
}
