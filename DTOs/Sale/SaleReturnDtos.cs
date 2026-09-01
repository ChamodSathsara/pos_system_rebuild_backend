namespace PosApi.DTOs.Sale;

public class CreateSaleReturnItemLineDto
{
    /// <summary>Must match an ItemCode already sold on the referenced invoice.</summary>
    public string ItemCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

/// <summary>
/// Returns previously sold items back into stock against a completed sale. Unit price for each
/// line is taken from the original SaleItem, not supplied by the caller, and the quantity
/// returned per item can never exceed what was sold minus what's already been returned. Posting
/// a return mirrors the sale in reverse, in one transaction: it inserts the return header/lines
/// and restores stock inventory/batches (crediting back the same batches the original sale drew
/// from, FIFO), raising an IN stock movement for each.
/// </summary>
public class CreateSaleReturnDto
{
    /// <summary>Optional. Auto-generated (e.g. SRT000001) when omitted.</summary>
    public string? ReturnNo { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime? ReturnDate { get; set; }
    public string? Reason { get; set; }
    public List<CreateSaleReturnItemLineDto> Items { get; set; } = new();
}

public class SaleReturnItemDto
{
    public int Id { get; set; }
    public string? ReturnNo { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalAmount { get; set; }
}

public class SaleReturnDto
{
    public string ReturnNo { get; set; } = string.Empty;
    public string? InvoiceNo { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Reason { get; set; }
    public decimal? TotalReturnAmount { get; set; }
    public string? CreatedBy { get; set; }
    public List<SaleReturnItemDto> Items { get; set; } = new();
}
