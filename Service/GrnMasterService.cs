using AutoMapper;
using PosApi.DTOs.Grn;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class GrnMasterService : IGrnMasterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GrnMasterService> _logger;

    public GrnMasterService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GrnMasterService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GrnDto>> SearchAsync(
        string? poNo,
        int? vendorId,
        string? branchCode,
        string? warehouseCode,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var grns = await _unitOfWork.GrnMasters.SearchAsync(poNo, vendorId, branchCode, warehouseCode, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<GrnDto>>(grns);
    }

    public async Task<GrnDto> GetByIdAsync(int grnId, CancellationToken cancellationToken = default)
    {
        var grn = await _unitOfWork.GrnMasters.GetByIdWithDetailsAsync(grnId, cancellationToken)
            ?? throw new NotFoundException("GrnMaster", grnId);

        return _mapper.Map<GrnDto>(grn);
    }

    public async Task<GrnDto> CreateAsync(CreateGrnDto request, string receivedBy, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new BadRequestException("A GRN must contain at least one line item.");
        }

        // ---- 1. Validate the source purchase order and header references ----
        var order = await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(request.PoNo, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", request.PoNo);

        if (order.Status is PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.FullyReceived)
        {
            throw new ConflictException($"Purchase order '{request.PoNo}' is {order.Status} and cannot receive any more GRNs.");
        }

        if (order.VendorId is null)
        {
            throw new ConflictException($"Purchase order '{request.PoNo}' has no vendor assigned.");
        }

        var vendor = await _unitOfWork.Vendors.GetByIdAsync(order.VendorId.Value, cancellationToken)
            ?? throw new NotFoundException("Vendor", order.VendorId.Value);

        if (!vendor.IsActive)
        {
            throw new BadRequestException($"Vendor '{vendor.VendorCode}' is inactive and cannot receive new GRNs.");
        }

        var branchCode = request.BranchCode.Trim();
        var warehouseCode = request.WarehouseCode.Trim();

        if (!await _unitOfWork.Branches.BranchCodeExistsAsync(branchCode, cancellationToken))
        {
            throw new BadRequestException($"Branch '{branchCode}' does not exist.");
        }

        if (!await _unitOfWork.Warehouses.WarehouseCodeExistsAsync(warehouseCode, cancellationToken))
        {
            throw new BadRequestException($"Warehouse '{warehouseCode}' does not exist.");
        }

        var grnNo = request.GrnNo?.Trim();
        if (string.IsNullOrWhiteSpace(grnNo))
        {
            grnNo = await _unitOfWork.GrnMasters.GenerateNextGrnNoAsync(cancellationToken);
        }
        else if (await _unitOfWork.GrnMasters.GrnNoExistsAsync(grnNo, cancellationToken))
        {
            throw new ConflictException($"A GRN with number '{grnNo}' already exists.");
        }

        var grnDate = request.GrnDate ?? DateTime.UtcNow;

        // ---- 2. Validate each line against the PO and build the GrnItem list ----
        var grnItems = new List<GrnItem>();
        decimal grnTotal = 0;

        foreach (var line in request.Items)
        {
            if (line.Quantity <= 0)
            {
                throw new BadRequestException($"Quantity for item '{line.ItemCode}' must be greater than zero.");
            }

            if (await _unitOfWork.Products.GetByIdAsync(line.ItemCode, cancellationToken) is null)
            {
                throw new BadRequestException($"Product '{line.ItemCode}' does not exist.");
            }

            var poItem = order.Items.FirstOrDefault(i => i.ItemCode == line.ItemCode)
                ?? throw new BadRequestException($"Item '{line.ItemCode}' is not part of purchase order '{request.PoNo}'.");

            var remaining = (poItem.Quantity ?? 0) - (poItem.ReceivedQuantity ?? 0);
            if (line.Quantity > remaining)
            {
                throw new BadRequestException(
                    $"Cannot receive {line.Quantity} of '{line.ItemCode}': only {remaining} remaining on purchase order '{request.PoNo}'.");
            }

            var totalCost = line.Quantity * line.UnitCost;
            grnTotal += totalCost;

            grnItems.Add(new GrnItem
            {
                ItemCode = line.ItemCode,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                TotalCost = totalCost,
                BatchNo = string.IsNullOrWhiteSpace(line.BatchNo) ? $"{grnNo}-{line.ItemCode}" : line.BatchNo.Trim(),
                ExpiryDate = line.ExpiryDate
            });

            // Reflect the receipt on the purchase order line immediately.
            poItem.ReceivedQuantity = (poItem.ReceivedQuantity ?? 0) + line.Quantity;
            _unitOfWork.PurchaseOrderItems.Update(poItem);
        }

        // ---- 3. Insert grn_master / grn_item ----
        var grn = new GrnMaster
        {
            GrnNo = grnNo,
            PoNo = request.PoNo,
            VendorId = order.VendorId,
            BranchCode = branchCode,
            WarehouseCode = warehouseCode,
            GrnDate = grnDate,
            InvoiceNo = request.InvoiceNo,
            InvoiceDate = request.InvoiceDate,
            TotalAmount = grnTotal,
            Remarks = request.Remarks,
            ReceivedBy = receivedBy,
            CreatedAt = DateTime.UtcNow,
            Items = grnItems
        };

        await _unitOfWork.GrnMasters.AddAsync(grn, cancellationToken);

        // ---- 4/5/6. Update stock_inventory, add stock_batch, add STOCK_IN stock_movement ----
        foreach (var grnItem in grnItems)
        {
            var stock = await _unitOfWork.StockInventories.GetByCombinationAsync(grnItem.ItemCode!, branchCode, warehouseCode, cancellationToken);
            var isNewStockLine = stock is null;

            if (isNewStockLine)
            {
                stock = new StockInventory
                {
                    ItemCode = grnItem.ItemCode!,
                    BranchCode = branchCode,
                    WarehouseCode = warehouseCode,
                    CurrentQty = 0,
                    LastUpdated = DateTime.UtcNow
                };
            }

            var previousQty = stock!.CurrentQty;
            var newQty = previousQty + (grnItem.Quantity ?? 0);

            stock.CurrentQty = newQty;
            stock.LastUpdated = DateTime.UtcNow;

            if (isNewStockLine)
            {
                // New entity: AddAsync tracks it as Added. Calling Update() as well would flip
                // that to Modified and break the insert, so it's skipped for the new-line path.
                await _unitOfWork.StockInventories.AddAsync(stock, cancellationToken);
            }
            else
            {
                _unitOfWork.StockInventories.Update(stock);
            }

            var batch = new StockBatch
            {
                StockInventory = stock,
                BatchNo = grnItem.BatchNo!,
                ReceivedQty = grnItem.Quantity ?? 0,
                AvailableQty = grnItem.Quantity ?? 0,
                UnitCost = grnItem.UnitCost ?? 0,
                ExpiryDate = grnItem.ExpiryDate,
                ReceivedDate = grnDate,
                Status = BatchStatus.Available
            };

            await _unitOfWork.StockBatches.AddAsync(batch, cancellationToken);

            var movement = new StockMovement
            {
                StockInventory = stock,
                StockBatch = batch,
                MovementType = StockMovementType.In,
                ReferenceType = StockReferenceType.Grn,
                ReferenceNo = grnNo,
                Qty = grnItem.Quantity ?? 0,
                PreviousQty = previousQty,
                NewQty = newQty,
                UnitCost = grnItem.UnitCost ?? 0,
                Remarks = $"GRN {grnNo} - item {grnItem.ItemCode} received into batch {grnItem.BatchNo}.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = receivedBy
            };

            await _unitOfWork.StockMovements.AddAsync(movement, cancellationToken);
        }

        // ---- 7. purchase_order_item.received_quantity already updated above; recompute PO status ----
        var previousStatus = order.Status;
        order.Status = ComputePoStatus(order);
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.PurchaseOrders.Update(order);

        // ---- 5 (continued). purchase_order_history ----
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
            PoNo = request.PoNo,
            Action = PurchaseOrderHistoryAction.StatusChanged,
            ChangedBy = receivedBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = $"GRN {grnNo} received against PO {request.PoNo}: {grnItems.Count} item(s), total {grnTotal:N2}.",
            Changes = historyChanges
        }, cancellationToken);

        // ---- 8. vendor_ledger ----
        var ledger = await _unitOfWork.VendorLedgers.GetByVendorIdAsync(order.VendorId.Value, cancellationToken)
            ?? throw new NotFoundException($"No ledger was found for vendor id '{order.VendorId.Value}'.");

        ledger.GrnTotal = (ledger.GrnTotal ?? 0) + grnTotal;
        ledger.OutstandingBalance = (ledger.GrnTotal ?? 0) - (ledger.ReturnTotal ?? 0) - (ledger.PaidCredit ?? 0);
        _unitOfWork.VendorLedgers.Update(ledger);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "GRN {GrnNo} posted against PO {PoNo}: {ItemCount} item(s), total {Total:N2}, PO status {Status}",
            grnNo, request.PoNo, grnItems.Count, grnTotal, order.Status);

        return await GetByIdAsync(grn.GrnId, cancellationToken);
    }

    public async Task DeleteAsync(int grnId, string updatedBy, CancellationToken cancellationToken = default)
    {
        var grn = await _unitOfWork.GrnMasters.GetByIdWithDetailsAsync(grnId, cancellationToken)
            ?? throw new NotFoundException("GrnMaster", grnId);

        if (await _unitOfWork.GrnMasters.HasReturnsAsync(grnId, cancellationToken))
        {
            throw new ConflictException($"GRN '{grn.GrnNo}' has returns recorded against it and cannot be deleted.");
        }

        var order = grn.PoNo is null
            ? null
            : await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(grn.PoNo, cancellationToken);

        foreach (var grnItem in grn.Items)
        {
            var stock = grn.BranchCode is null || grn.WarehouseCode is null
                ? null
                : await _unitOfWork.StockInventories.GetByCombinationAsync(grnItem.ItemCode!, grn.BranchCode, grn.WarehouseCode, cancellationToken);

            var batch = stock is null || grnItem.BatchNo is null
                ? null
                : await _unitOfWork.StockBatches.GetByStockIdAndBatchNoAsync(stock.StockId, grnItem.BatchNo, cancellationToken);

            if (batch is not null && batch.AvailableQty != batch.ReceivedQty)
            {
                throw new ConflictException(
                    $"Batch '{batch.BatchNo}' from GRN '{grn.GrnNo}' has already been partially consumed or adjusted and cannot be reversed.");
            }

            if (stock is not null)
            {
                stock.CurrentQty -= grnItem.Quantity ?? 0;
                if (stock.CurrentQty < 0)
                {
                    stock.CurrentQty = 0;
                }
                stock.LastUpdated = DateTime.UtcNow;
                _unitOfWork.StockInventories.Update(stock);
            }

            if (batch is not null)
            {
                var movements = await _unitOfWork.StockMovements.FindAsync(m => m.BatchId == batch.BatchId, cancellationToken);
                foreach (var movement in movements)
                {
                    _unitOfWork.StockMovements.Remove(movement);
                }

                _unitOfWork.StockBatches.Remove(batch);
            }

            if (order is not null)
            {
                var poItem = order.Items.FirstOrDefault(i => i.ItemCode == grnItem.ItemCode);
                if (poItem is not null)
                {
                    poItem.ReceivedQuantity = (poItem.ReceivedQuantity ?? 0) - (grnItem.Quantity ?? 0);
                    if (poItem.ReceivedQuantity < 0)
                    {
                        poItem.ReceivedQuantity = 0;
                    }
                    _unitOfWork.PurchaseOrderItems.Update(poItem);
                }
            }
        }

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
                ChangedBy = updatedBy,
                ChangedAt = DateTime.UtcNow,
                Remarks = $"GRN {grn.GrnNo} deleted and reversed against PO {order.PoNo}.",
                Changes = historyChanges
            }, cancellationToken);
        }

        if (grn.VendorId is not null)
        {
            var ledger = await _unitOfWork.VendorLedgers.GetByVendorIdAsync(grn.VendorId.Value, cancellationToken);
            if (ledger is not null)
            {
                ledger.GrnTotal = (ledger.GrnTotal ?? 0) - (grn.TotalAmount ?? 0);
                if (ledger.GrnTotal < 0)
                {
                    ledger.GrnTotal = 0;
                }
                ledger.OutstandingBalance = (ledger.GrnTotal ?? 0) - (ledger.ReturnTotal ?? 0) - (ledger.PaidCredit ?? 0);
                _unitOfWork.VendorLedgers.Update(ledger);
            }
        }

        // Items cascade-delete via EF configuration.
        _unitOfWork.GrnMasters.Remove(grn);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("GRN {GrnNo} (id {GrnId}) deleted and reversed successfully", grn.GrnNo, grnId);
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
