using System.Globalization;
using System.Net;
using System.Text;
using PosApi.DTOs.Sale;

namespace PosApi.Helpers;

/// <summary>
/// Renders a <see cref="SaleInvoiceDto"/> as an 80mm-thermal-ready receipt. Two output shapes are
/// supported from the same data: a raw monospace text layout (<see cref="ToText"/>), suitable for
/// piping straight to a receipt printer, and a printable HTML page (<see cref="ToHtml"/>) sized
/// for an 80mm roll (72-76mm printable width, 2-4mm margins) with the TOTAL line bolded/enlarged,
/// for browser "Print" to a thermal printer driver.
/// </summary>
public static class InvoicePrintFormatter
{
    /// <summary>Character width of the text receipt - matches a standard 80mm thermal printer's line length.</summary>
    private const int LineWidth = 40;

    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static string ToText(SaleInvoiceDto invoice)
    {
        var sb = new StringBuilder();
        var divider = new string('=', LineWidth);
        var dashes = new string('-', LineWidth);

        sb.AppendLine(divider);
        if (!string.IsNullOrWhiteSpace(invoice.CompanyName))
        {
            sb.AppendLine(Center(invoice.CompanyName!.ToUpper(Culture)));
        }
        if (!string.IsNullOrWhiteSpace(invoice.BranchAddress ?? invoice.CompanyAddress))
        {
            sb.AppendLine(Center(invoice.BranchAddress ?? invoice.CompanyAddress!));
        }
        var phone = invoice.BranchPhone ?? invoice.CompanyPhone;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            sb.AppendLine(Center($"Tel: {phone}"));
        }
        sb.AppendLine(divider);

        sb.AppendLine(Field("INVOICE NO", invoice.InvoiceNo));
        sb.AppendLine(Field("DATE", invoice.SaleDate?.ToString("dd/MM/yyyy  hh:mm tt", Culture) ?? "-"));
        sb.AppendLine(Field("BRANCH", invoice.BranchName ?? invoice.BranchCode ?? "-"));
        sb.AppendLine(Field("CASHIER", invoice.CashierName ?? invoice.CashierCode ?? "-"));
        sb.AppendLine(dashes);

        sb.AppendLine(ItemRow("ITEM", "QTY", "PRICE", "LP", "AMT"));
        sb.AppendLine(dashes);
        foreach (var item in invoice.Items)
        {
            sb.AppendLine(ItemRow(
                Truncate(item.ItemName ?? item.ItemCode ?? "-", 12),
                FormatQty(item.Quantity),
                item.Price.ToString("N0", Culture),
                item.Lp.ToString("N0", Culture),
                item.Amount.ToString("N0", Culture)));
        }
        sb.AppendLine(dashes);

        sb.AppendLine(Amount("SUB TOTAL", invoice.Subtotal));
        if (invoice.DiscountAmount != 0)
        {
            sb.AppendLine(Amount("DISCOUNT", invoice.DiscountAmount));
        }
        if (invoice.TaxAmount != 0)
        {
            sb.AppendLine(Amount("TAX", invoice.TaxAmount));
        }
        sb.AppendLine(PadRight(LineWidth - 8) + "--------");
        sb.AppendLine(Amount("TOTAL", invoice.TotalAmount));
        sb.AppendLine(PadRight(LineWidth - 8) + "========");

        foreach (var payment in invoice.Payments)
        {
            sb.AppendLine(Amount(payment.PaymentMethod.ToString().ToUpper(Culture), payment.Amount));
        }
        sb.AppendLine(Amount(invoice.BalanceAmount > 0 ? "BALANCE DUE" : "BALANCE", Math.Abs(invoice.BalanceAmount)));
        sb.AppendLine(dashes);

        sb.AppendLine($"Customer : {invoice.CustomerName ?? "Walk-in Customer"}");
        sb.AppendLine(dashes);
        sb.AppendLine(Center("*  Thank You!  *"));
        sb.AppendLine(Center("Please Come Again"));
        sb.AppendLine(divider);

        return sb.ToString();
    }

    public static string ToHtml(SaleInvoiceDto invoice)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Invoice {Enc(invoice.InvoiceNo)}</title>");
        sb.Append("""
            <style>
                @page { size: 80mm auto; margin: 2mm; }
                * { box-sizing: border-box; }
                body {
                    width: 76mm;
                    margin: 0 auto;
                    padding: 2mm;
                    font-family: 'Courier New', Consolas, monospace;
                    font-size: 9pt;
                    line-height: 1.3;
                    color: #000;
                }
                .center { text-align: center; }
                .title { font-size: 13pt; font-weight: bold; }
                hr { border: none; border-top: 1px dashed #000; margin: 2mm 0; }
                .double { border-top: 1px solid #000; border-bottom: 1px solid #000; padding: 1px 0; }
                table { width: 100%; border-collapse: collapse; font-size: 8pt; }
                th, td { text-align: right; padding: 1px 0; }
                th:first-child, td:first-child { text-align: left; }
                .row { display: flex; justify-content: space-between; }
                .total-row { display: flex; justify-content: space-between; font-size: 12pt; font-weight: bold; }
                .footer { margin-top: 2mm; }
                @media print { body { width: 76mm; } }
            </style>
            """);
        sb.Append("</head><body>");

        sb.Append("<div class=\"center title\">").Append(Enc(invoice.CompanyName?.ToUpper(Culture) ?? "")).Append("</div>");
        var address = invoice.BranchAddress ?? invoice.CompanyAddress;
        if (!string.IsNullOrWhiteSpace(address))
        {
            sb.Append("<div class=\"center\">").Append(Enc(address)).Append("</div>");
        }
        var phone = invoice.BranchPhone ?? invoice.CompanyPhone;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            sb.Append("<div class=\"center\">Tel: ").Append(Enc(phone)).Append("</div>");
        }
        sb.Append("<hr/>");

        sb.Append("<div class=\"row\"><span>INVOICE NO</span><span>").Append(Enc(invoice.InvoiceNo)).Append("</span></div>");
        sb.Append("<div class=\"row\"><span>DATE</span><span>")
          .Append(Enc(invoice.SaleDate?.ToString("dd/MM/yyyy  hh:mm tt", Culture) ?? "-")).Append("</span></div>");
        sb.Append("<div class=\"row\"><span>BRANCH</span><span>").Append(Enc(invoice.BranchName ?? invoice.BranchCode)).Append("</span></div>");
        sb.Append("<div class=\"row\"><span>CASHIER</span><span>").Append(Enc(invoice.CashierName ?? invoice.CashierCode)).Append("</span></div>");
        sb.Append("<hr/>");

        sb.Append("<table><thead><tr><th>ITEM</th><th>QTY</th><th>PRICE</th><th>LP</th><th>AMT</th></tr></thead><tbody>");
        foreach (var item in invoice.Items)
        {
            sb.Append("<tr><td>").Append(Enc(item.ItemName ?? item.ItemCode)).Append("</td><td>")
              .Append(FormatQty(item.Quantity)).Append("</td><td>")
              .Append(item.Price.ToString("N2", Culture)).Append("</td><td>")
              .Append(item.Lp.ToString("N2", Culture)).Append("</td><td>")
              .Append(item.Amount.ToString("N2", Culture)).Append("</td></tr>");
        }
        sb.Append("</tbody></table><hr/>");

        sb.Append("<div class=\"row\"><span>SUB TOTAL</span><span>").Append(invoice.Subtotal.ToString("N2", Culture)).Append("</span></div>");
        if (invoice.DiscountAmount != 0)
        {
            sb.Append("<div class=\"row\"><span>DISCOUNT</span><span>").Append(invoice.DiscountAmount.ToString("N2", Culture)).Append("</span></div>");
        }
        if (invoice.TaxAmount != 0)
        {
            sb.Append("<div class=\"row\"><span>TAX</span><span>").Append(invoice.TaxAmount.ToString("N2", Culture)).Append("</span></div>");
        }
        sb.Append("<div class=\"double total-row\"><span>TOTAL</span><span>").Append(invoice.TotalAmount.ToString("N2", Culture)).Append("</span></div>");

        foreach (var payment in invoice.Payments)
        {
            sb.Append("<div class=\"row\"><span>").Append(Enc(payment.PaymentMethod.ToString().ToUpper(Culture)))
              .Append("</span><span>").Append(payment.Amount.ToString("N2", Culture)).Append("</span></div>");
        }
        var balanceLabel = invoice.BalanceAmount > 0 ? "BALANCE DUE" : "BALANCE";
        sb.Append("<div class=\"row\"><span>").Append(balanceLabel).Append("</span><span>")
          .Append(Math.Abs(invoice.BalanceAmount).ToString("N2", Culture)).Append("</span></div>");
        sb.Append("<hr/>");

        sb.Append("<div>Customer : ").Append(Enc(invoice.CustomerName ?? "Walk-in Customer")).Append("</div>");
        sb.Append("<hr/>");
        sb.Append("<div class=\"center footer\">&#9733; Thank You! &#9733;<br/>Please Come Again</div>");

        sb.Append("<script>window.onload = function () { window.print(); };</script>");
        sb.Append("</body></html>");

        return sb.ToString();
    }

    private static string Field(string label, string value) => $"{label,-11}: {value}";

    private static string Amount(string label, decimal value)
    {
        var amountText = value.ToString("N2", Culture);
        var padded = label.PadRight(LineWidth - amountText.Length);
        return padded + amountText;
    }

    private static string ItemRow(string name, string qty, string price, string lp, string amt)
        => $"{Truncate(name, 12),-12}{qty,5}{price,8}{lp,7}{amt,7}";

    private static string FormatQty(decimal qty) => qty == Math.Truncate(qty)
        ? qty.ToString("N0", Culture)
        : qty.ToString("N2", Culture);

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static string Center(string text)
    {
        if (text.Length >= LineWidth)
        {
            return text[..LineWidth];
        }

        var totalPad = LineWidth - text.Length;
        var left = totalPad / 2;
        return new string(' ', left) + text;
    }

    private static string PadRight(int count) => new(' ', Math.Max(count, 0));

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
