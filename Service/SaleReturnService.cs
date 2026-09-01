using AutoMapper;
using PosApi.DTOs.Sale;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class SaleReturnService : ISaleReturnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SaleReturnService> _logger;

    public SaleReturnService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SaleReturnService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SaleReturnDto>> SearchAsync(
        string? invoiceNo,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var returns = await _unitOfWork.SaleReturns.SearchAsync(invoiceNo, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<SaleReturnDto>>(returns);
    }

    public async Task<SaleReturnDto> GetByIdAsync(string returnNo, CancellationToken cancellationToken = default)
    {
        var saleReturn = await _unitOfWork.SaleReturns.GetByIdWithDetailsAsync(returnNo, cancellationToken)
            ?? throw new NotFoundException("SaleReturn", returnNo);

        return _mapper.Map<SaleReturnDto>(saleReturn);
    }

    public async Task<SaleReturnDto> CreateAsync(CreateSaleReturnDto request, string createdBy, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new BadRequestException("A sale return must contain at least one line item.");
        }

        // ---- 1. Validate the source sale ----
        var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(request.InvoiceNo, cancellationToken)
            ?? throw new NotFoundException("Sale", request.InvoiceNo);

        if (sale.Status != SaleStatus.Completed)
        {
            throw new ConflictException($"Sale '{request.InvoiceNo}' is {sale.Status} and cannot accept returns.");
        }

        if (string.IsNullOrWhiteSpace(sale.BranchCode))
        {
            throw new ConflictException($"Sale '{request.InvoiceNo}' has no branch recorded and cannot be returned.");
        }

        var returnNo = request.ReturnNo?.Trim();
        if (string.IsNullOrWhiteSpace(returnNo))
        {
            returnNo = await _unitOfWork.SaleReturns.GenerateNextReturnNoAsync(cancellationToken);
        }
        else if (await _unitOfWork.SaleReturns.ReturnNoExistsAsync(returnNo, cancellationToken))
        {
            throw new ConflictException($"A sale return with number '{returnNo}' already exists.");
        }

        var returnDate = request.ReturnDate ?? DateTime.UtcNow;

        // ---- 2. Validate each line against what was sold and build the SaleReturnItem list ----
        var returnItems = new List<SaleReturnItem>();
        decimal returnTotal = 0;

        foreach (var line in request.Items)
        {
            if (line.Quantity <= 0)
            {
                throw new BadRequestException("Return quantity must be greater than zero.");
            }

            var soldItems = sale.Items.Where(i => i.ItemCode == line.ItemCode).ToList();
            if (soldItems.Count == 0)
            {
                throw new BadRequestException($"Item '{line.ItemCode}' is not part of invoice '{request.InvoiceNo}'.");
            }

            var soldQty = soldItems.Sum(i => i.Quantity ?? 0);
            var alreadyReturned = await _unitOfWork.SaleReturns.GetReturnedQuantityForItemAsync(request.InvoiceNo, line.ItemCode, cancellationToken);
            var returnable = soldQty - alreadyReturned;

            if (line.Quantity > returnable)
            {
                throw new BadRequestException(
                    $"Cannot return {line.Quantity} of '{line.ItemCode}': only {returnable} still returnable from invoice '{request.InvoiceNo}'.");
            }

            var unitPrice = soldItems[0].UnitPrice ?? 0;
            var totalAmount = line.Quantity * unitPrice;
            returnTotal += totalAmount;

            returnItems.Add(new SaleReturnItem
            {
                ItemCode = line.ItemCode,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                TotalAmount = totalAmount
            });
        }

        // ---- 3. Insert sale_return / sale_return_item ----
        var saleReturn = new SaleReturn
        {
            ReturnNo = returnNo,
            InvoiceNo = request.InvoiceNo,
            ReturnDate = returnDate,
            Reason = request.Reason,
            TotalReturnAmount = returnTotal,
            CreatedBy = createdBy,
            Items = returnItems
        };

        await _unitOfWork.SaleReturns.AddAsync(saleReturn, cancellationToken);

        // ---- 4. Restore stock: credit back the same batches the original sale drew from, FIFO ----
        foreach (var returnItem in returnItems)
        {
            await RestoreStockFifoAsync(
                returnItem.ItemCode!,
                request.InvoiceNo,
                returnItem.Quantity ?? 0,
                returnNo,
                createdBy,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Sale return {ReturnNo} posted against invoice {InvoiceNo}: {ItemCount} item(s), total {Total:N2}",
            returnNo, request.InvoiceNo, returnItems.Count, returnTotal);

        return await GetByIdAsync(returnNo, cancellationToken);
    }

    /// <summary>
    /// Restores the given quantity of an item back into stock by crediting the same batches the
    /// original sale drew it from, oldest-consumed-first, raising an IN stock movement per batch
    /// touched.
    /// </summary>
    private async Task RestoreStockFifoAsync(
        string itemCode,
        string invoiceNo,
        decimal quantity,
        string returnNo,
        string createdBy,
        CancellationToken cancellationToken)
    {
        var movements = await _unitOfWork.StockMovements.SearchAsync(null, null, invoiceNo, cancellationToken);
        var saleOutMovements = movements
            .Where(m => m.MovementType == StockMovementType.Out && m.ReferenceType == StockReferenceType.Sale)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        var remaining = quantity;

        foreach (var movement in saleOutMovements)
        {
            if (remaining <= 0)
            {
                break;
            }

            var batch = await _unitOfWork.StockBatches.GetByIdAsync(movement.BatchId, cancellationToken);
            var stock = await _unitOfWork.StockInventories.GetByIdAsync(movement.StockId, cancellationToken);

            if (batch is null || stock is null || stock.ItemCode != itemCode)
            {
                continue;
            }

            var consumedInBatch = batch.ReceivedQty - batch.AvailableQty;
            if (consumedInBatch <= 0)
            {
                continue;
            }

            var restoreAmount = Math.Min(remaining, consumedInBatch);

            batch.AvailableQty += restoreAmount;
            if (batch.AvailableQty > batch.ReceivedQty)
            {
                batch.AvailableQty = batch.ReceivedQty;
            }
            if (batch.Status == BatchStatus.Completed && batch.AvailableQty > 0)
            {
                batch.Status = BatchStatus.Available;
            }
            _unitOfWork.StockBatches.Update(batch);

            var previousQty = stock.CurrentQty;
            stock.CurrentQty += restoreAmount;
            stock.LastUpdated = DateTime.UtcNow;
            _unitOfWork.StockInventories.Update(stock);

            await _unitOfWork.StockMovements.AddAsync(new StockMovement
            {
                StockInventory = stock,
                StockBatch = batch,
                MovementType = StockMovementType.In,
                ReferenceType = StockReferenceType.SaleReturn,
                ReferenceNo = returnNo,
                Qty = restoreAmount,
                PreviousQty = previousQty,
                NewQty = stock.CurrentQty,
                UnitCost = batch.UnitCost,
                Remarks = $"Sale Return {returnNo} against {invoiceNo} - item {itemCode} restored to batch {batch.BatchNo}.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            }, cancellationToken);

            remaining -= restoreAmount;
        }

        if (remaining > 0)
        {
            throw new ConflictException(
                $"Could not trace {remaining} of item '{itemCode}' back to a stock batch drawn from invoice '{invoiceNo}'.");
        }
    }
}
