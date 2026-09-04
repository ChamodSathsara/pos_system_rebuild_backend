using AutoMapper;
using PosApi.DTOs.Sale;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class SaleService : ISaleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SaleService> _logger;

    public SaleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SaleService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SaleDto>> SearchAsync(
        string? branchCode,
        string? customerCode,
        SaleStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var sales = await _unitOfWork.Sales.SearchAsync(branchCode, customerCode, status, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<SaleDto>>(sales);
    }

    public async Task<SaleDto> GetByIdAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(invoiceNo, cancellationToken)
            ?? throw new NotFoundException("Sale", invoiceNo);

        return _mapper.Map<SaleDto>(sale);
    }

    public async Task<SaleDto> CreateAsync(CreateSaleDto request, string createdBy, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new BadRequestException("A sale must contain at least one line item.");
        }

        // ---- 1. Validate header references ----
        if (!await _unitOfWork.Branches.BranchCodeExistsAsync(request.BranchCode, cancellationToken))
        {
            throw new BadRequestException($"Branch '{request.BranchCode}' does not exist.");
        }

        Customer? customer = null;
        if (!string.IsNullOrWhiteSpace(request.CustomerCode))
        {
            customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerCode, cancellationToken)
                ?? throw new NotFoundException("Customer", request.CustomerCode);

            if (!customer.IsActive)
            {
                throw new BadRequestException($"Customer '{request.CustomerCode}' is inactive and cannot be billed.");
            }
        }

        var invoiceNo = request.InvoiceNo?.Trim();
        if (string.IsNullOrWhiteSpace(invoiceNo))
        {
            invoiceNo = await _unitOfWork.Sales.GenerateNextInvoiceNoAsync(cancellationToken);
        }
        else if (await _unitOfWork.Sales.InvoiceNoExistsAsync(invoiceNo, cancellationToken))
        {
            throw new ConflictException($"A sale with invoice number '{invoiceNo}' already exists.");
        }

        var saleDate = request.SaleDate ?? DateTime.UtcNow;

        // ---- 2. Validate each line, price it against the product, and build the SaleItem list ----
        var saleItems = new List<SaleItem>();
        decimal subtotal = 0;
        decimal lineDiscountTotal = 0;
        decimal taxTotal = 0;

        var itemCodes = request.Items
            .Select(x => x.ItemCode.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var products = (await _unitOfWork.Products.GetByIdsWithDetailsAsync(itemCodes, cancellationToken))
            .ToDictionary(x => x.ItemCode, StringComparer.OrdinalIgnoreCase);

        foreach (var line in request.Items)
        {
            if (line.Quantity <= 0)
            {
                throw new BadRequestException($"Quantity for item '{line.ItemCode}' must be greater than zero.");
            }

            var normalizedItemCode = line.ItemCode.Trim();
            if (!products.TryGetValue(normalizedItemCode, out var product))
            {
                throw new BadRequestException($"Product '{line.ItemCode}' does not exist.");
            }

            if (!product.IsActive)
            {
                throw new BadRequestException($"Product '{line.ItemCode}' is inactive and cannot be sold.");
            }

            var unitPrice = line.UnitPrice ?? product.SellingPrice ?? 0;
            var lineDiscount = line.DiscountAmount ?? 0;
            var lineSubtotal = line.Quantity * unitPrice;

            if (lineDiscount > lineSubtotal)
            {
                throw new BadRequestException($"Discount for item '{line.ItemCode}' cannot exceed its line subtotal.");
            }

            var taxPercentage = product.Tax?.Percentage ?? 0;
            var taxableAmount = lineSubtotal - lineDiscount;
            var lineTax = taxableAmount * taxPercentage / 100m;
            var lineTotal = taxableAmount + lineTax;

            subtotal += lineSubtotal;
            lineDiscountTotal += lineDiscount;
            taxTotal += lineTax;

            saleItems.Add(new SaleItem
            {
                ItemCode = product.ItemCode,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                DiscountAmount = lineDiscount,
                TaxAmount = lineTax,
                TotalPrice = lineTotal
            });
        }

        var headerDiscount = request.DiscountAmount ?? 0;
        var totalDiscount = lineDiscountTotal + headerDiscount;
        var totalAmount = subtotal - totalDiscount + taxTotal;
        if (totalAmount < 0)
        {
            totalAmount = 0;
        }

        // ---- 3. Draw down stock FIFO across the branch's available batches for every line ----
        var availableBatches = await _unitOfWork.StockBatches
            .GetAvailableBatchesByItemsAndBranchAsync(itemCodes, request.BranchCode, cancellationToken);
        var batchesByItem = availableBatches
            .GroupBy(x => x.StockInventory!.ItemCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<StockBatch>)x.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var saleItem in saleItems)
        {
            batchesByItem.TryGetValue(saleItem.ItemCode!, out var batches);
            await ConsumeStockFifoAsync(
                saleItem.ItemCode!,
                saleItem.Quantity ?? 0,
                invoiceNo,
                createdBy,
                batches ?? Array.Empty<StockBatch>(),
                cancellationToken);
        }

        // ---- 4. Insert sale / sale_item ----
        var sale = new Sale
        {
            InvoiceNo = invoiceNo,
            BranchCode = request.BranchCode,
            CustomerCode = string.IsNullOrWhiteSpace(request.CustomerCode) ? null : request.CustomerCode,
            Customer = customer,
            SaleDate = saleDate,
            Subtotal = subtotal,
            DiscountAmount = totalDiscount,
            TaxAmount = taxTotal,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            BalanceAmount = totalAmount,
            Status = SaleStatus.Completed,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            Items = saleItems
        };

        await _unitOfWork.Sales.AddAsync(sale, cancellationToken);

        // ---- 5. Record any initial payments supplied at the point of sale ----
        foreach (var paymentLine in request.Payments)
        {
            if (paymentLine.Amount <= 0)
            {
                throw new BadRequestException("Payment amount must be greater than zero.");
            }

            if (paymentLine.Amount > sale.BalanceAmount)
            {
                throw new BadRequestException(
                    $"Payment of {paymentLine.Amount:N2} exceeds the sale's balance of {sale.BalanceAmount:N2}.");
            }

            await _unitOfWork.Payments.AddAsync(new Payment
            {
                InvoiceNo = invoiceNo,
                PaymentMethod = paymentLine.PaymentMethod,
                Amount = paymentLine.Amount,
                PaymentDate = DateTime.UtcNow,
                ReferenceNo = paymentLine.ReferenceNo,
                Status = PaymentStatus.Completed,
                ReceivedBy = createdBy
            }, cancellationToken);

            sale.PaidAmount = (sale.PaidAmount ?? 0) + paymentLine.Amount;
            sale.BalanceAmount = (sale.BalanceAmount ?? 0) - paymentLine.Amount;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var saleItem in saleItems)
        {
            saleItem.Product = products[saleItem.ItemCode!];
        }

        _logger.LogInformation(
            "Sale {InvoiceNo} posted for branch {BranchCode}: {ItemCount} item(s), total {Total:N2}",
            invoiceNo, request.BranchCode, saleItems.Count, totalAmount);

        // The complete aggregate is already tracked in memory; avoid reloading it from the
        // database immediately after SaveChanges. Generated line IDs are populated by EF.
        return _mapper.Map<SaleDto>(sale);
    }

    public async Task<SaleInvoiceDto> GetInvoiceAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(invoiceNo, cancellationToken)
            ?? throw new NotFoundException("Sale", invoiceNo);

        var branch = sale.Branch;
        var company = branch?.Company;

        var invoice = new SaleInvoiceDto
        {
            InvoiceNo = sale.InvoiceNo,
            SaleDate = sale.SaleDate,

            CompanyName = company?.CompanyName,
            CompanyAddress = company?.Address,
            CompanyPhone = company?.Phone,

            BranchCode = sale.BranchCode,
            BranchName = branch?.BranchName,
            BranchAddress = branch?.Address,
            BranchPhone = branch?.Phone,

            CashierCode = sale.CreatedBy,
            CashierName = sale.CreatedByUser?.FullName ?? sale.CreatedByUser?.Username,

            CustomerCode = sale.CustomerCode,
            CustomerName = sale.Customer?.CustomerName,

            Subtotal = sale.Subtotal ?? 0,
            DiscountAmount = sale.DiscountAmount ?? 0,
            TaxAmount = sale.TaxAmount ?? 0,
            TotalAmount = sale.TotalAmount ?? 0,
            PaidAmount = sale.PaidAmount ?? 0,
            BalanceAmount = sale.BalanceAmount ?? 0
        };

        foreach (var item in sale.Items)
        {
            var quantity = item.Quantity ?? 0;
            var price = item.UnitPrice ?? 0;
            var discount = item.DiscountAmount ?? 0;

            // "Last Price" = the effective per-unit price once the line's discount is applied.
            var lp = quantity > 0 ? Math.Round(price - (discount / quantity), 2) : price;
            var amount = quantity > 0 ? Math.Round((quantity * price) - discount, 2) : 0;

            invoice.Items.Add(new SaleInvoiceItemDto
            {
                ItemCode = item.ItemCode,
                ItemName = item.Product?.ItemName ?? item.ItemCode,
                Quantity = quantity,
                Price = price,
                Lp = lp,
                Amount = amount
            });
        }

        foreach (var payment in sale.Payments
                     .Where(p => p.Status == PaymentStatus.Completed)
                     .OrderBy(p => p.PaymentDate))
        {
            invoice.Payments.Add(new SaleInvoicePaymentDto
            {
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount ?? 0,
                ReferenceNo = payment.ReferenceNo
            });
        }

        return invoice;
    }

    public async Task<SaleDto> CancelAsync(string invoiceNo, string cancelledBy, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(invoiceNo, cancellationToken)
            ?? throw new NotFoundException("Sale", invoiceNo);

        if (sale.Status != SaleStatus.Completed)
        {
            throw new ConflictException($"Sale '{invoiceNo}' is {sale.Status} and cannot be cancelled.");
        }

        if (await _unitOfWork.Sales.HasReturnsAsync(invoiceNo, cancellationToken))
        {
            throw new ConflictException($"Sale '{invoiceNo}' has returns recorded against it and cannot be cancelled.");
        }

        var payments = await _unitOfWork.Payments.GetByInvoiceNoAsync(invoiceNo, cancellationToken);
        if (payments.Any(p => p.Status == PaymentStatus.Completed))
        {
            throw new ConflictException($"Sale '{invoiceNo}' has payments recorded against it - void the payments first.");
        }

        // ---- Reverse every stock-out movement raised when the sale was posted ----
        var movements = await _unitOfWork.StockMovements.SearchAsync(null, null, invoiceNo, cancellationToken);
        var saleOutMovements = movements
            .Where(m => m.MovementType == StockMovementType.Out && m.ReferenceType == StockReferenceType.Sale)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        foreach (var movement in saleOutMovements)
        {
            var batch = await _unitOfWork.StockBatches.GetByIdAsync(movement.BatchId, cancellationToken);
            var stock = await _unitOfWork.StockInventories.GetByIdAsync(movement.StockId, cancellationToken);

            if (batch is null || stock is null)
            {
                continue;
            }

            var reversalAmount = -movement.Qty;

            batch.AvailableQty += reversalAmount;
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
            stock.CurrentQty += reversalAmount;
            stock.LastUpdated = DateTime.UtcNow;
            _unitOfWork.StockInventories.Update(stock);

            await _unitOfWork.StockMovements.AddAsync(new StockMovement
            {
                StockInventory = stock,
                StockBatch = batch,
                MovementType = StockMovementType.In,
                ReferenceType = StockReferenceType.Sale,
                ReferenceNo = invoiceNo,
                Qty = reversalAmount,
                PreviousQty = previousQty,
                NewQty = stock.CurrentQty,
                UnitCost = movement.UnitCost,
                Remarks = $"Sale {invoiceNo} cancelled - stock restored to batch {batch.BatchNo}.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = cancelledBy
            }, cancellationToken);
        }

        sale.Status = SaleStatus.Cancelled;
        _unitOfWork.Sales.Update(sale);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Sale {InvoiceNo} cancelled by {CancelledBy} and stock restored", invoiceNo, cancelledBy);

        return await GetByIdAsync(invoiceNo, cancellationToken);
    }

    /// <summary>Draws down the given quantity of an item FIFO across the branch's available batches, raising a stock-out movement for each batch touched.</summary>
    private async Task ConsumeStockFifoAsync(
        string itemCode,
        decimal quantity,
        string invoiceNo,
        string createdBy,
        IReadOnlyList<StockBatch> batches,
        CancellationToken cancellationToken)
    {
        if (batches.Sum(b => b.AvailableQty) < quantity)
        {
            throw new BadRequestException(
                $"Insufficient stock for item '{itemCode}': requested {quantity}, only {batches.Sum(b => b.AvailableQty)} available.");
        }

        var remaining = quantity;

        foreach (var batch in batches)
        {
            if (remaining <= 0)
            {
                break;
            }

            var stock = batch.StockInventory
                ?? throw new ConflictException($"Batch '{batch.BatchNo}' has no owning stock line and cannot be drawn down.");

            var take = Math.Min(remaining, batch.AvailableQty);

            batch.AvailableQty -= take;
            if (batch.AvailableQty <= 0)
            {
                batch.AvailableQty = 0;
                batch.Status = BatchStatus.Completed;
            }
            _unitOfWork.StockBatches.Update(batch);

            var previousQty = stock.CurrentQty;
            stock.CurrentQty -= take;
            if (stock.CurrentQty < 0)
            {
                stock.CurrentQty = 0;
            }
            stock.LastUpdated = DateTime.UtcNow;
            _unitOfWork.StockInventories.Update(stock);

            await _unitOfWork.StockMovements.AddAsync(new StockMovement
            {
                StockInventory = stock,
                StockBatch = batch,
                MovementType = StockMovementType.Out,
                ReferenceType = StockReferenceType.Sale,
                ReferenceNo = invoiceNo,
                Qty = -take,
                PreviousQty = previousQty,
                NewQty = stock.CurrentQty,
                UnitCost = batch.UnitCost,
                Remarks = $"Sale {invoiceNo} - item {itemCode} drawn from batch {batch.BatchNo}.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            }, cancellationToken);

            remaining -= take;
        }
    }
}
