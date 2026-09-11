using System.Data;
using Microsoft.EntityFrameworkCore;
using PosApi.Constants;
using PosApi.Data;
using PosApi.DTOs.Stock;
using PosApi.Exceptions;
using PosApi.Helpers;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;
using QuestPDF.Fluent;

namespace PosApi.Service;

public class CentralStockReceiptService(ApplicationDbContext context, ILogger<CentralStockReceiptService> logger) : ICentralStockReceiptService
{
    public async Task<IReadOnlyList<CentralStockReceiptDto>> SearchAsync(string? warehouseCode, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var query = BaseQuery();
        if (!string.IsNullOrWhiteSpace(warehouseCode)) query = query.Where(x => x.WarehouseCode == warehouseCode.Trim());
        if (fromDate.HasValue) query = query.Where(x => x.ReceiptDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(x => x.ReceiptDate < toDate.Value.Date.AddDays(1));
        return (await query.OrderByDescending(x => x.ReceiptDate).ThenByDescending(x => x.ReceiptId).ToListAsync(ct)).Select(ToDto).ToList();
    }

    public async Task<CentralStockReceiptDto> GetByIdAsync(long receiptId, CancellationToken ct = default)
    {
        var receipt = await BaseQuery().SingleOrDefaultAsync(x => x.ReceiptId == receiptId, ct)
            ?? throw new NotFoundException("CentralStockReceipt", receiptId);
        return ToDto(receipt);
    }

    public async Task<CentralStockReceiptDto> CreateAsync(CreateCentralStockReceiptDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default)
    {
        var warehouseCode = request.WarehouseCode.Trim();
        var warehouse = await context.Warehouses.SingleOrDefaultAsync(x => x.WarehouseCode == warehouseCode, ct)
            ?? throw new NotFoundException("Warehouse", warehouseCode);
        if (!warehouse.IsCentralWarehouse) throw new BadRequestException("Central stock can only be received into a Main/Central Warehouse.");

        var isGlobal = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);
        if (!isGlobal && (!string.Equals(role, "InventoryClerk", StringComparison.OrdinalIgnoreCase) || !string.Equals(userWarehouseCode, warehouseCode, StringComparison.OrdinalIgnoreCase)))
            throw new ForbiddenAppException("You can only receive stock into your assigned Central Warehouse.");

        var itemCodes = request.Items.Select(x => x.ItemCode.Trim()).ToList();
        var products = await context.Products.Where(x => itemCodes.Contains(x.ItemCode)).ToDictionaryAsync(x => x.ItemCode, StringComparer.OrdinalIgnoreCase, ct);
        var missing = itemCodes.FirstOrDefault(x => !products.ContainsKey(x));
        if (missing is not null) throw new NotFoundException("ProductMaster", missing);

        var receiptNo = $"CSR{await NextSequenceAsync("central_stock_receipt_sequence", ct):D6}";
        var receiptDate = request.ReceiptDate ?? DateTime.UtcNow;
        var now = DateTime.UtcNow;
        var receipt = new CentralStockReceipt
        {
            ReceiptNo = receiptNo,
            WarehouseCode = warehouseCode,
            ReceiptDate = receiptDate,
            ReferenceNo = Clean(request.ReferenceNo),
            Remarks = Clean(request.Remarks),
            TotalQuantity = request.Items.Sum(x => x.Quantity),
            TotalCost = request.Items.Sum(x => x.Quantity * x.UnitCost),
            ReceivedBy = userCode,
            CreatedAt = now
        };

        foreach (var requestLine in request.Items)
        {
            var itemCode = requestLine.ItemCode.Trim();
            var product = products[itemCode];
            var stock = await context.StockInventories.SingleOrDefaultAsync(x => x.ItemCode == itemCode && x.BranchCode == null && x.WarehouseCode == warehouseCode, ct);
            if (stock is null)
            {
                stock = new StockInventory { ItemCode = itemCode, BranchCode = null, WarehouseCode = warehouseCode, CurrentQty = 0, LastUpdated = now };
                context.StockInventories.Add(stock);
            }

            var previousQty = stock.CurrentQty;
            stock.CurrentQty += requestLine.Quantity;
            stock.LastUpdated = now;
            product.SellingPrice = requestLine.SellingPrice;
            product.UpdatedAt = now;

            var batchNo = $"CB{await NextSequenceAsync("opening_stock_batch_sequence", ct):D6}";
            var batch = new StockBatch
            {
                StockInventory = stock,
                BatchNo = batchNo,
                ReceivedQty = requestLine.Quantity,
                AvailableQty = requestLine.Quantity,
                UnitCost = requestLine.UnitCost,
                SellingPrice = requestLine.SellingPrice,
                ExpiryDate = requestLine.ExpiryDate,
                ReceivedDate = receiptDate,
                Status = BatchStatus.Available
            };
            context.StockBatches.Add(batch);

            context.StockMovements.Add(new StockMovement
            {
                StockBatch = batch,
                StockInventory = stock,
                MovementType = StockMovementType.In,
                ReferenceType = StockReferenceType.CentralStockReceipt,
                ReferenceNo = receiptNo,
                Qty = requestLine.Quantity,
                PreviousQty = previousQty,
                NewQty = stock.CurrentQty,
                UnitCost = requestLine.UnitCost,
                Remarks = Clean(request.Remarks) ?? $"Central stock received under {receiptNo}.",
                CreatedAt = now,
                CreatedBy = userCode
            });
            context.ItemLogs.Add(ItemLogFactory.Create(itemCode, ItemLogActions.StockChanged, previousQty.ToString(), stock.CurrentQty.ToString(), userCode));
            receipt.Lines.Add(new CentralStockReceiptLine
            {
                ItemCode = itemCode,
                Batch = batch,
                Quantity = requestLine.Quantity,
                UnitCost = requestLine.UnitCost,
                SellingPrice = requestLine.SellingPrice,
                ExpiryDate = requestLine.ExpiryDate
            });
        }

        context.CentralStockReceipts.Add(receipt);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Central stock receipt {ReceiptNo} added {Quantity} units to {WarehouseCode} by {UserCode}", receiptNo, receipt.TotalQuantity, warehouseCode, userCode);
        return await GetByIdAsync(receipt.ReceiptId, ct);
    }

    public async Task<(string FileName, byte[] Content)> GetReceiptPdfAsync(long receiptId, CancellationToken ct = default)
    {
        var receipt = await BaseQuery().SingleOrDefaultAsync(x => x.ReceiptId == receiptId, ct)
            ?? throw new NotFoundException("CentralStockReceipt", receiptId);
        var pdf = Document.Create(document => document.Page(page =>
        {
            page.Margin(25);
            page.Header().Text("CENTRAL WAREHOUSE STOCK RECEIPT").FontSize(18).Bold();
            page.Content().Column(column =>
            {
                column.Spacing(8);
                column.Item().Text($"Receipt: {receipt.ReceiptNo}    Date: {receipt.ReceiptDate:yyyy-MM-dd HH:mm}");
                column.Item().Text($"Warehouse: {receipt.Warehouse!.WarehouseName} ({receipt.WarehouseCode})");
                column.Item().Text($"Reference: {receipt.ReferenceNo ?? "-"}    Received by: {receipt.ReceivedBy}");
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                    table.Header(h => { h.Cell().Text("Item").Bold(); h.Cell().Text("Batch").Bold(); h.Cell().Text("Qty").Bold(); h.Cell().Text("Unit Cost").Bold(); h.Cell().Text("Total").Bold(); });
                    foreach (var line in receipt.Lines)
                    {
                        table.Cell().Text($"{line.Product?.ItemName ?? line.ItemCode} ({line.ItemCode})");
                        table.Cell().Text(line.Batch!.BatchNo);
                        table.Cell().Text(line.Quantity.ToString("0.###"));
                        table.Cell().Text(line.UnitCost.ToString("N2"));
                        table.Cell().Text((line.Quantity * line.UnitCost).ToString("N2"));
                    }
                });
                column.Item().AlignRight().Text($"Total Quantity: {receipt.TotalQuantity:0.###}    Total Cost: {receipt.TotalCost:N2}").Bold();
                if (!string.IsNullOrWhiteSpace(receipt.Remarks)) column.Item().Text($"Remarks: {receipt.Remarks}");
            });
        })).GeneratePdf();
        return ($"{receipt.ReceiptNo}-CentralStockReceipt.pdf", pdf);
    }

    private IQueryable<CentralStockReceipt> BaseQuery() => context.CentralStockReceipts.AsNoTracking()
        .Include(x => x.Warehouse)
        .Include(x => x.Lines).ThenInclude(x => x.Product)
        .Include(x => x.Lines).ThenInclude(x => x.Batch);

    private async Task<int> NextSequenceAsync(string sequenceName, CancellationToken ct)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        try
        {
            if (shouldClose) await connection.OpenAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT NEXT VALUE FOR dbo.{sequenceName}";
            return Convert.ToInt32(await command.ExecuteScalarAsync(ct));
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    private static CentralStockReceiptDto ToDto(CentralStockReceipt receipt) => new()
    {
        ReceiptId = receipt.ReceiptId,
        ReceiptNo = receipt.ReceiptNo,
        WarehouseCode = receipt.WarehouseCode,
        WarehouseName = receipt.Warehouse?.WarehouseName,
        ReceiptDate = receipt.ReceiptDate,
        ReferenceNo = receipt.ReferenceNo,
        Remarks = receipt.Remarks,
        TotalQuantity = receipt.TotalQuantity,
        TotalCost = receipt.TotalCost,
        ReceivedBy = receipt.ReceivedBy,
        CreatedAt = receipt.CreatedAt,
        Items = receipt.Lines.Select(x => new CentralStockReceiptLineDto
        {
            ReceiptLineId = x.ReceiptLineId,
            ItemCode = x.ItemCode,
            ItemName = x.Product?.ItemName,
            BatchId = x.BatchId,
            BatchNo = x.Batch?.BatchNo ?? string.Empty,
            Quantity = x.Quantity,
            UnitCost = x.UnitCost,
            SellingPrice = x.SellingPrice,
            TotalCost = x.Quantity * x.UnitCost,
            ExpiryDate = x.ExpiryDate
        }).ToList()
    };

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
