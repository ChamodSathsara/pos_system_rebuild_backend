using AutoMapper;
using PosApi.DTOs.Grn;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class GrnReturnService : IGrnReturnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GrnReturnService> _logger;

    public GrnReturnService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GrnReturnService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GrnReturnDto>> SearchAsync(
        int? grnId,
        GrnReturnStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var returns = await _unitOfWork.GrnReturns.SearchAsync(grnId, status, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<GrnReturnDto>>(returns);
    }

    public async Task<GrnReturnDto> GetByIdAsync(int grnReturnId, CancellationToken cancellationToken = default)
    {
        var grnReturn = await _unitOfWork.GrnReturns.GetByIdWithDetailsAsync(grnReturnId, cancellationToken)
            ?? throw new NotFoundException("GrnReturn", grnReturnId);

        return _mapper.Map<GrnReturnDto>(grnReturn);
    }

    public async Task<GrnReturnDto> CreateAsync(CreateGrnReturnDto request, string returnBy, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new BadRequestException("A GRN return must contain at least one line item.");
        }

        // ---- 1. Validate the source GRN and its purchase order ----
        var grn = await _unitOfWork.GrnMasters.GetByIdWithDetailsAsync(request.GrnId, cancellationToken)
            ?? throw new NotFoundException("GrnMaster", request.GrnId);

        if (grn.BranchCode is null || grn.WarehouseCode is null)
        {
            throw new ConflictException($"GRN '{grn.GrnNo}' has no branch/warehouse recorded and cannot be returned.");
        }

        var order = grn.PoNo is null
            ? null
            : await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(grn.PoNo, cancellationToken);

        var returnDate = request.ReturnDate ?? DateTime.UtcNow;

        // ---- 2. Validate each line against the original GRN item and build the GrnReturnItem list ----
        var returnItems = new List<GrnReturnItem>();
        decimal returnTotal = 0;

        foreach (var line in request.Items)
        {
            if (line.Quantity <= 0)
            {
                throw new BadRequestException("Return quantity must be greater than zero.");
            }

            var grnItem = grn.Items.FirstOrDefault(i => i.GrnItemId == line.GrnItemId)
                ?? throw new BadRequestException($"GrnItem {line.GrnItemId} does not belong to GRN '{grn.GrnNo}'.");

            var alreadyReturned = await _unitOfWork.GrnReturns.GetReturnedQuantityForGrnItemAsync(grnItem.GrnItemId, cancellationToken);
            var returnable = (grnItem.Quantity ?? 0) - alreadyReturned;

            if (line.Quantity > returnable)
            {
                throw new BadRequestException(
                    $"Cannot return {line.Quantity} of '{grnItem.ItemCode}': only {returnable} still returnable from GRN '{grn.GrnNo}'.");
            }

            var totalAmount = line.Quantity * (grnItem.UnitCost ?? 0);
            returnTotal += totalAmount;

            returnItems.Add(new GrnReturnItem
            {
                GrnItem = grnItem,
                ItemCode = grnItem.ItemCode,
                Quantity = line.Quantity,
                UnitCost = grnItem.UnitCost,
                TotalAmount = totalAmount
            });
        }

        // ---- 3. Insert grn_return / grn_return_item ----
        var grnReturn = new GrnReturn
        {
            GrnId = grn.GrnId,
            ReturnDate = returnDate,
            ReturnBy = returnBy,
            TotalReturnAmount = returnTotal,
            Reason = request.Reason,
            Status = GrnReturnStatus.Completed,
            Items = returnItems
        };

        await _unitOfWork.GrnReturns.AddAsync(grnReturn, cancellationToken);

        // ---- 4/5/6. Update stock_inventory, draw down stock_batch, add STOCK_OUT stock_movement ----
        foreach (var returnItem in returnItems)
        {
            var grnItem = returnItem.GrnItem!;

            var stock = await _unitOfWork.StockInventories.GetByCombinationAsync(grnItem.ItemCode!, grn.BranchCode, grn.WarehouseCode, cancellationToken)
                ?? throw new ConflictException($"No stock line found for item '{grnItem.ItemCode}' at branch '{grn.BranchCode}' / warehouse '{grn.WarehouseCode}'.");

            var batch = grnItem.BatchNo is null
                ? null
                : await _unitOfWork.StockBatches.GetByStockIdAndBatchNoAsync(stock.StockId, grnItem.BatchNo, cancellationToken);

            if (batch is null)
            {
                throw new ConflictException($"Batch '{grnItem.BatchNo}' from GRN '{grn.GrnNo}' could not be found and cannot be returned.");
            }

            if (batch.AvailableQty < returnItem.Quantity)
            {
                throw new BadRequestException(
                    $"Insufficient available quantity in batch '{batch.BatchNo}': requested {returnItem.Quantity}, only {batch.AvailableQty} available.");
            }

            var previousQty = stock.CurrentQty;
            var newQty = previousQty - (returnItem.Quantity ?? 0);
            if (newQty < 0)
            {
                newQty = 0;
            }

            stock.CurrentQty = newQty;
            stock.LastUpdated = DateTime.UtcNow;
            _unitOfWork.StockInventories.Update(stock);

            batch.AvailableQty -= returnItem.Quantity ?? 0;
            if (batch.AvailableQty <= 0)
            {
                batch.AvailableQty = 0;
                if (batch.Status == BatchStatus.Available)
                {
                    batch.Status = BatchStatus.Completed;
                }
            }
            _unitOfWork.StockBatches.Update(batch);

            var movement = new StockMovement
            {
                StockInventory = stock,
                StockBatch = batch,
                MovementType = StockMovementType.Out,
                ReferenceType = StockReferenceType.GrnReturn,
                ReferenceNo = grn.GrnNo,
                Qty = -(returnItem.Quantity ?? 0),
                PreviousQty = previousQty,
                NewQty = newQty,
                UnitCost = returnItem.UnitCost ?? 0,
                Remarks = $"GRN Return against {grn.GrnNo} - item {grnItem.ItemCode} returned from batch {batch.BatchNo}.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = returnBy
            };

            await _unitOfWork.StockMovements.AddAsync(movement, cancellationToken);

            // ---- 7. purchase_order_item.received_quantity rollback ----
            if (order is not null)
            {
                var poItem = order.Items.FirstOrDefault(i => i.ItemCode == grnItem.ItemCode);
                if (poItem is not null)
                {
                    poItem.ReceivedQuantity = (poItem.ReceivedQuantity ?? 0) - (returnItem.Quantity ?? 0);
                    if (poItem.ReceivedQuantity < 0)
                    {
                        poItem.ReceivedQuantity = 0;
                    }
                    _unitOfWork.PurchaseOrderItems.Update(poItem);
                }
            }
        }

        // ---- 6 (continued) / 5. purchase_order status + purchase_order_history ----
        if (order is not null)
        {
            var previousStatus = order.Status;
            order.Status = ComputePoStatus(order);
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PurchaseOrders.Update(order);

            var historyChanges = new List<PurchaseOrderHistoryChange>();
            if (order.Status != previousStatus)
            {
                historyChanges.Add(new PurchaseOrderHistoryChange
                {
                    Field = PurchaseOrderChangeField.Status,
                    OldValue = previousStatus.ToString(),
                    NewValue = order.Status.ToString()
                });
            }

            await _unitOfWork.PurchaseOrderHistories.AddAsync(new PurchaseOrderHistory
            {
                PoNo = order.PoNo,
                Action = PurchaseOrderHistoryAction.StatusChanged,
                ChangedBy = returnBy,
                ChangedAt = DateTime.UtcNow,
                Remarks = $"GRN Return recorded against PO {order.PoNo} for GRN {grn.GrnNo}: {returnItems.Count} item(s), total {returnTotal:N2}.",
                Changes = historyChanges
            }, cancellationToken);
        }

        // ---- 8. vendor_ledger ----
        if (grn.VendorId is not null)
        {
            var ledger = await _unitOfWork.VendorLedgers.GetByVendorIdAsync(grn.VendorId.Value, cancellationToken)
                ?? throw new NotFoundException($"No ledger was found for vendor id '{grn.VendorId.Value}'.");

            ledger.ReturnTotal = (ledger.ReturnTotal ?? 0) + returnTotal;
            ledger.OutstandingBalance = (ledger.GrnTotal ?? 0) - (ledger.ReturnTotal ?? 0) - (ledger.PaidCredit ?? 0);
            _unitOfWork.VendorLedgers.Update(ledger);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "GRN Return posted against GRN {GrnNo}: {ItemCount} item(s), total {Total:N2}",
            grn.GrnNo, returnItems.Count, returnTotal);

        return await GetByIdAsync(grnReturn.GrnReturnId, cancellationToken);
    }

    private static PurchaseOrderStatus ComputePoStatus(PurchaseOrder order)
    {
        var totalOrdered = order.Items.Sum(i => i.Quantity ?? 0);
        var totalReceived = order.Items.Sum(i => i.ReceivedQuantity ?? 0);

        if (totalReceived <= 0)
        {
            return PurchaseOrderStatus.Open;
        }

        return totalReceived >= totalOrdered ? PurchaseOrderStatus.FullyReceived : PurchaseOrderStatus.PartiallyReceived;
    }
}
