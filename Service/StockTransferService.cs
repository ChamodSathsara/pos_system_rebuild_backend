using Microsoft.EntityFrameworkCore;
using PosApi.Data;
using PosApi.Constants;
using PosApi.DTOs.Stock;
using PosApi.Exceptions;
using PosApi.Helpers;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PosApi.Service;

/// <summary>Transactional central-warehouse to branch-warehouse transfer workflow.</summary>
public class StockTransferService(ApplicationDbContext context, ILogger<StockTransferService> logger) : IStockTransferService
{
    public async Task<StockTransferRequestDto> CreateAsync(CreateStockTransferRequestDto request, string userCode, string? userBranchCode, string? role, CancellationToken ct = default)
    {
        if (request.Lines.Count == 0 || request.Lines.Any(x => string.IsNullOrWhiteSpace(x.ItemCode) || x.Quantity <= 0)) throw new BadRequestException("At least one item with a quantity greater than zero is required.");
        var source = await WarehouseAsync(request.SourceWarehouseCode, ct);
        var destination = await WarehouseAsync(request.DestinationWarehouseCode, ct);
        if (!source.IsCentralWarehouse) throw new BadRequestException("The source warehouse must be configured as a central warehouse.");
        if (source.WarehouseCode == destination.WarehouseCode) throw new BadRequestException("Source and destination warehouses must be different.");
        if (!IsGlobalRole(role) && !string.Equals(destination.BranchCode, userBranchCode, StringComparison.OrdinalIgnoreCase)) throw new ForbiddenAppException("You can only create stock requests for your assigned branch.");
        var itemCodes = request.Lines.Select(x => x.ItemCode.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (itemCodes.Count != request.Lines.Count || await context.Products.CountAsync(x => itemCodes.Contains(x.ItemCode), ct) != itemCodes.Count) throw new BadRequestException("Every requested item must exist and be included only once.");
        var now = DateTime.UtcNow;
        var entity = new StockTransferRequest
        {
            RequestNo = $"STR{now:yyyyMMddHHmmssfff}", SourceWarehouseCode = source.WarehouseCode, DestinationWarehouseCode = destination.WarehouseCode,
            RequestDate = now, RequiredDate = request.RequiredDate, Status = StockTransferStatus.Submitted, Remarks = Clean(request.Remarks), RequestedBy = userCode, CreatedAt = now,
            Lines = request.Lines.Select(x => new StockTransferRequestLine { ItemCode = x.ItemCode.Trim(), RequestedQty = x.Quantity, Remarks = Clean(x.Remarks) }).ToList()
        };
        context.StockTransferRequests.Add(entity); await context.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<IReadOnlyList<StockTransferRequestDto>> GetRequestsAsync(string? sourceWarehouseCode, string? destinationWarehouseCode, CancellationToken ct = default)
    {
        var query = context.StockTransferRequests.AsNoTracking().Include(x => x.Lines).Include(x => x.SourceWarehouse).Include(x => x.DestinationWarehouse).AsQueryable();
        if (!string.IsNullOrWhiteSpace(sourceWarehouseCode)) query = query.Where(x => x.SourceWarehouseCode == sourceWarehouseCode.Trim());
        if (!string.IsNullOrWhiteSpace(destinationWarehouseCode)) query = query.Where(x => x.DestinationWarehouseCode == destinationWarehouseCode.Trim());
        var transfers = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        var ids = transfers.Select(x => x.TransferRequestId).ToList();
        var poNumbers = await context.PurchaseOrders.AsNoTracking()
            .Where(x => x.TransferRequestId.HasValue && ids.Contains(x.TransferRequestId.Value))
            .ToDictionaryAsync(x => x.TransferRequestId!.Value, x => x.PoNo, ct);
        return transfers.Select(x =>
        {
            var dto = ToDto(x);
            dto.LinkedPoNo = poNumbers.GetValueOrDefault(x.TransferRequestId);
            return dto;
        }).ToList();
    }

    public async Task<StockTransferRequestDto> GetRequestByIdAsync(long id, CancellationToken ct = default)
    {
        var transfer = await context.StockTransferRequests.AsNoTracking()
            .Include(x => x.SourceWarehouse)
            .Include(x => x.DestinationWarehouse)
            .Include(x => x.Lines)
            .Include(x => x.Dispatches).ThenInclude(x => x.Lines).ThenInclude(x => x.TransferRequestLine)
            .Include(x => x.Dispatches).ThenInclude(x => x.Lines).ThenInclude(x => x.StockBatch)
            .SingleOrDefaultAsync(x => x.TransferRequestId == id, ct)
            ?? throw new NotFoundException("StockTransferRequest", id);

        var result = ToDto(transfer);
        result.LinkedPoNo = await context.PurchaseOrders.AsNoTracking()
            .Where(x => x.TransferRequestId == id)
            .Select(x => x.PoNo)
            .SingleOrDefaultAsync(ct);
        return result;
    }

    public async Task<StockTransferRequestDto> AcceptAsync(long id, AcceptStockTransferRequestDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default)
    {
        var transfer = await RequestAsync(id, ct);
        EnsureWarehouseAccess(transfer.SourceWarehouseCode, userWarehouseCode, role);
        if (transfer.Status != StockTransferStatus.Submitted) throw new ConflictException("Only submitted transfer requests can be accepted.");
        if (request.Lines.Count == 0) throw new BadRequestException("Approved quantities are required.");
        foreach (var line in transfer.Lines)
        {
            var approved = request.Lines.SingleOrDefault(x => x.TransferRequestLineId == line.TransferRequestLineId)?.ApprovedQty ?? 0;
            if (approved < 0 || approved > line.RequestedQty) throw new BadRequestException($"Approved quantity for '{line.ItemCode}' must be between zero and requested quantity.");
            line.ApprovedQty = approved;
        }
        if (transfer.Lines.All(x => x.ApprovedQty == 0)) throw new BadRequestException("At least one line must be approved.");
        transfer.Status = StockTransferStatus.Accepted; transfer.AcceptedBy = userCode; transfer.AcceptedAt = DateTime.UtcNow; transfer.UpdatedAt = DateTime.UtcNow; transfer.Remarks = Clean(request.Remarks) ?? transfer.Remarks;
        await context.SaveChangesAsync(ct); return ToDto(transfer);
    }

    public async Task<StockTransferDispatchDto> DispatchAsync(long id, CreateStockTransferDispatchDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default)
    {
        if (request.Lines.Count == 0 || request.Lines.Any(x => x.Quantity <= 0)) throw new BadRequestException("At least one batch and dispatch quantity are required.");
        var transfer = await RequestAsync(id, ct);
        EnsureWarehouseAccess(transfer.SourceWarehouseCode, userWarehouseCode, role);
        if (transfer.Status is not StockTransferStatus.Accepted and not StockTransferStatus.Picking) throw new ConflictException("Only accepted requests can be dispatched.");
        var duplicateLines = request.Lines.GroupBy(x => new { x.TransferRequestLineId, x.BatchId }).Any(x => x.Count() > 1);
        if (duplicateLines) throw new BadRequestException("A request line/batch combination can only be dispatched once.");
        var now = DateTime.UtcNow;
        var dispatch = new StockTransferDispatch { DispatchNo = $"DN{now:yyyyMMddHHmmssfff}", TransferRequestId = transfer.TransferRequestId, VehicleNo = Clean(request.VehicleNo), DriverName = Clean(request.DriverName), Remarks = Clean(request.Remarks), DispatchedBy = userCode, DispatchedAt = now };
        context.StockTransferDispatches.Add(dispatch);
        foreach (var dto in request.Lines)
        {
            var line = transfer.Lines.SingleOrDefault(x => x.TransferRequestLineId == dto.TransferRequestLineId) ?? throw new BadRequestException("A dispatch line does not belong to this transfer request.");
            if (line.DispatchedQty + dto.Quantity > line.ApprovedQty) throw new BadRequestException($"Dispatch quantity for '{line.ItemCode}' exceeds the approved quantity.");
            var batch = await context.StockBatches.Include(x => x.StockInventory).SingleOrDefaultAsync(x => x.BatchId == dto.BatchId, ct) ?? throw new NotFoundException("StockBatch", dto.BatchId);
            if (batch.StockInventory!.WarehouseCode != transfer.SourceWarehouseCode || batch.StockInventory.ItemCode != line.ItemCode) throw new BadRequestException("Selected batch does not belong to the requested item in the source warehouse.");
            if (batch.Status != BatchStatus.Available || batch.AvailableQty < dto.Quantity) throw new ConflictException($"Insufficient available quantity in batch '{batch.BatchNo}'.");
            var stock = batch.StockInventory; var previous = stock.CurrentQty;
            batch.AvailableQty -= dto.Quantity; if (batch.AvailableQty == 0) batch.Status = BatchStatus.Completed;
            stock.CurrentQty -= dto.Quantity; stock.LastUpdated = now; line.DispatchedQty += dto.Quantity;
            context.StockMovements.Add(new StockMovement { BatchId = batch.BatchId, StockId = stock.StockId, MovementType = StockMovementType.Out, ReferenceType = StockReferenceType.StockTransfer, ReferenceNo = dispatch.DispatchNo, Qty = -dto.Quantity, PreviousQty = previous, NewQty = stock.CurrentQty, UnitCost = batch.UnitCost, Remarks = $"Dispatched to {transfer.DestinationWarehouseCode}", CreatedAt = now, CreatedBy = userCode });
            dispatch.Lines.Add(new StockTransferDispatchLine { TransferRequestLineId = line.TransferRequestLineId, BatchId = batch.BatchId, Quantity = dto.Quantity, UnitCost = batch.UnitCost });
            context.ItemLogs.Add(ItemLogFactory.Create(stock.ItemCode, ItemLogActions.StockChanged, previous.ToString(), stock.CurrentQty.ToString(), userCode));
        }
        var linkedPo = await context.PurchaseOrders.Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.TransferRequestId == transfer.TransferRequestId, ct);
        if (linkedPo is not null)
        {
            foreach (var poItem in linkedPo.Items)
            {
                var dispatched = dispatch.Lines.Where(x => x.TransferRequestLine?.ItemCode == poItem.ItemCode).ToList();
                if (dispatched.Count == 0)
                {
                    var transferLineIds = transfer.Lines.Where(x => x.ItemCode == poItem.ItemCode).Select(x => x.TransferRequestLineId).ToHashSet();
                    dispatched = dispatch.Lines.Where(x => transferLineIds.Contains(x.TransferRequestLineId)).ToList();
                }
                var dispatchedQty = dispatched.Sum(x => x.Quantity);
                if (dispatchedQty > 0)
                {
                    poItem.UnitCost = dispatched.Sum(x => x.Quantity * x.UnitCost) / dispatchedQty;
                    poItem.TotalCost = (poItem.Quantity ?? 0) * poItem.UnitCost;
                }
            }
            linkedPo.TotalAmount = linkedPo.Items.Sum(x => x.TotalCost ?? 0);
            linkedPo.UpdatedAt = now;
        }
        transfer.Status = StockTransferStatus.Dispatched; transfer.UpdatedAt = now;
        // SaveChanges creates its own transaction. Do not start a user transaction here because
        // SQL Server retry execution strategy rejects it outside ExecuteAsync.
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Transfer {RequestNo} dispatched as {DispatchNo} by {UserCode}", transfer.RequestNo, dispatch.DispatchNo, userCode);
        return ToDto(dispatch);
    }

    public async Task<StockTransferDispatchDto> DirectDispatchAsync(CreateDirectStockTransferDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userWarehouseCode)) throw new ForbiddenAppException("Your user account is not assigned to a main warehouse.");
        if (request.Lines.Count == 0 || request.Lines.Any(x => string.IsNullOrWhiteSpace(x.ItemCode) || x.BatchId <= 0 || x.Quantity <= 0))
            throw new BadRequestException("At least one item, batch and quantity greater than zero is required.");

        var source = await WarehouseAsync(userWarehouseCode, ct);
        EnsureWarehouseAccess(source.WarehouseCode, userWarehouseCode, role);
        if (!source.IsCentralWarehouse) throw new BadRequestException("Direct transfers can only be dispatched from a central warehouse.");
        var destination = await WarehouseAsync(request.DestinationWarehouseCode, ct);
        if (destination.IsCentralWarehouse || string.IsNullOrWhiteSpace(destination.BranchCode)) throw new BadRequestException("The destination must be a branch warehouse.");
        if (source.WarehouseCode == destination.WarehouseCode) throw new BadRequestException("Source and destination warehouses must be different.");
        if (request.Lines.Select(x => x.ItemCode.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != request.Lines.Count)
            throw new BadRequestException("An item can only appear once in a direct transfer.");

        var now = DateTime.UtcNow;
        var transfer = new StockTransferRequest
        {
            RequestNo = $"DIR{now:yyyyMMddHHmmssfff}", SourceWarehouseCode = source.WarehouseCode, DestinationWarehouseCode = destination.WarehouseCode,
            RequestDate = now, Status = StockTransferStatus.Accepted, Remarks = Clean(request.Remarks), RequestedBy = userCode,
            AcceptedBy = userCode, AcceptedAt = now, CreatedAt = now,
            Lines = request.Lines.Select(x => new StockTransferRequestLine { ItemCode = x.ItemCode.Trim(), RequestedQty = x.Quantity, ApprovedQty = x.Quantity, Remarks = Clean(x.Remarks) }).ToList()
        };
        context.StockTransferRequests.Add(transfer);
        await context.SaveChangesAsync(ct);

        return await DispatchAsync(transfer.TransferRequestId, new CreateStockTransferDispatchDto
        {
            VehicleNo = request.VehicleNo, DriverName = request.DriverName, Remarks = request.Remarks,
            Lines = request.Lines.Select(x => new CreateStockTransferDispatchLineDto
            {
                TransferRequestLineId = transfer.Lines.Single(line => line.ItemCode == x.ItemCode.Trim()).TransferRequestLineId,
                BatchId = x.BatchId, Quantity = x.Quantity
            }).ToList()
        }, userCode, userWarehouseCode, role, ct);
    }

    public async Task<StockTransferRequestDto> CreateDirectProposalAsync(CreateDirectTransferProposalDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userWarehouseCode)) throw new ForbiddenAppException("Your user account is not assigned to a main warehouse.");
        if (request.Lines.Count == 0 || request.Lines.Any(x => string.IsNullOrWhiteSpace(x.ItemCode) || x.Quantity <= 0)) throw new BadRequestException("At least one item and quantity greater than zero is required.");
        var source = await WarehouseAsync(userWarehouseCode, ct);
        EnsureWarehouseAccess(source.WarehouseCode, userWarehouseCode, role);
        if (!source.IsCentralWarehouse) throw new BadRequestException("Direct transfers can only be created from a central warehouse.");
        var destination = await WarehouseAsync(request.DestinationWarehouseCode, ct);
        if (destination.IsCentralWarehouse || string.IsNullOrWhiteSpace(destination.BranchCode)) throw new BadRequestException("The destination must be a branch warehouse.");
        if (source.WarehouseCode == destination.WarehouseCode) throw new BadRequestException("Source and destination warehouses must be different.");
        var codes = request.Lines.Select(x => x.ItemCode.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (codes.Count != request.Lines.Count || await context.Products.CountAsync(x => codes.Contains(x.ItemCode), ct) != codes.Count) throw new BadRequestException("Every requested item must exist and be included only once.");

        var now = DateTime.UtcNow;
        var transfer = new StockTransferRequest
        {
            RequestNo = $"DIR{now:yyyyMMddHHmmssfff}", SourceWarehouseCode = source.WarehouseCode, DestinationWarehouseCode = destination.WarehouseCode,
            RequestDate = now, Status = StockTransferStatus.AwaitingBranch, Remarks = Clean(request.Remarks), RequestedBy = userCode, CreatedAt = now,
            Lines = request.Lines.Select(x => new StockTransferRequestLine { ItemCode = x.ItemCode.Trim(), RequestedQty = x.Quantity, Remarks = Clean(x.Remarks) }).ToList()
        };
        context.StockTransferRequests.Add(transfer);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Direct transfer proposal {RequestNo} created by {UserCode} for branch warehouse {Destination}", transfer.RequestNo, userCode, destination.WarehouseCode);
        return ToDto(transfer);
    }

    public async Task<StockTransferRequestDto> BranchAcceptAsync(long id, BranchTransferDecisionDto request, string userCode, string? userBranchCode, string? role, CancellationToken ct = default)
    {
        var transfer = await RequestAsync(id, ct);
        if (transfer.Status != StockTransferStatus.AwaitingBranch) throw new ConflictException("Only transfers awaiting branch acceptance can be accepted by a branch.");
        var destination = await WarehouseAsync(transfer.DestinationWarehouseCode, ct);
        if (!IsGlobalRole(role) && !string.Equals(destination.BranchCode, userBranchCode, StringComparison.OrdinalIgnoreCase)) throw new ForbiddenAppException("You can only accept transfers for your assigned branch.");
        foreach (var line in transfer.Lines) line.ApprovedQty = line.RequestedQty;
        transfer.Status = StockTransferStatus.Accepted;
        transfer.AcceptedBy = userCode;
        transfer.AcceptedAt = DateTime.UtcNow;
        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.Remarks = Clean(request.Remarks) ?? transfer.Remarks;
        var existingPo = await context.PurchaseOrders.AnyAsync(x => x.TransferRequestId == transfer.TransferRequestId, ct);
        if (!existingPo)
        {
            var poNo = await GenerateNextPoNoAsync(ct);
            var now = DateTime.UtcNow;
            context.PurchaseOrders.Add(new PurchaseOrder
            {
                PoNo = poNo,
                VendorId = null,
                SourceWarehouseCode = transfer.SourceWarehouseCode,
                DestinationWarehouseCode = transfer.DestinationWarehouseCode,
                IsInternalTransfer = true,
                TransferRequestId = transfer.TransferRequestId,
                BranchCode = destination.BranchCode,
                PoDate = now,
                ExpectedDate = transfer.RequiredDate,
                TotalAmount = 0,
                Remarks = transfer.Remarks,
                Status = PurchaseOrderStatus.Open,
                CreatedBy = userCode,
                CreatedAt = now,
                UpdatedAt = now,
                Items = transfer.Lines.Select(x => new PurchaseOrderItem
                {
                    ItemCode = x.ItemCode,
                    Quantity = x.RequestedQty,
                    ReceivedQuantity = 0,
                    UnitCost = 0,
                    TotalCost = 0
                }).ToList(),
                Histories = new List<PurchaseOrderHistory>
                {
                    new()
                    {
                        Action = PurchaseOrderHistoryAction.Created,
                        ChangedBy = userCode,
                        ChangedAt = now,
                        Remarks = $"Internal PO automatically created from accepted direct transfer {transfer.RequestNo}."
                    }
                }
            });
        }
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Direct transfer proposal {RequestNo} accepted by branch user {UserCode}", transfer.RequestNo, userCode);
        var result = ToDto(transfer);
        result.LinkedPoNo = await context.PurchaseOrders.AsNoTracking()
            .Where(x => x.TransferRequestId == transfer.TransferRequestId)
            .Select(x => x.PoNo)
            .SingleAsync(ct);
        return result;
    }

    public async Task<StockTransferRequestDto> BranchRejectAsync(long id, BranchTransferDecisionDto request, string userCode, string? userBranchCode, string? role, CancellationToken ct = default)
    {
        var transfer = await RequestAsync(id, ct);
        if (transfer.Status != StockTransferStatus.AwaitingBranch) throw new ConflictException("Only transfers awaiting branch acceptance can be rejected by a branch.");
        var destination = await WarehouseAsync(transfer.DestinationWarehouseCode, ct);
        if (!IsGlobalRole(role) && !string.Equals(destination.BranchCode, userBranchCode, StringComparison.OrdinalIgnoreCase)) throw new ForbiddenAppException("You can only reject transfers for your assigned branch.");

        transfer.Status = StockTransferStatus.Rejected;
        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.Remarks = Clean(request.Remarks) ?? transfer.Remarks;
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Direct transfer proposal {RequestNo} rejected by branch user {UserCode}", transfer.RequestNo, userCode);
        return ToDto(transfer);
    }

    public async Task<StockTransferReceiptDto> ReceiveAsync(long dispatchId, ReceiveStockTransferDispatchDto request, string userCode, string? userBranchCode, string? role, CancellationToken ct = default)
    {
        var dispatch = await context.StockTransferDispatches.Include(x => x.TransferRequest).ThenInclude(x => x!.Lines).Include(x => x.Lines).ThenInclude(x => x.StockBatch).ThenInclude(x => x!.StockInventory).SingleOrDefaultAsync(x => x.DispatchId == dispatchId, ct) ?? throw new NotFoundException("StockTransferDispatch", dispatchId);
        if (dispatch.Receipt is not null || await context.StockTransferReceipts.AnyAsync(x => x.DispatchId == dispatchId, ct)) throw new ConflictException("This dispatch has already been received.");
        var transfer = dispatch.TransferRequest!;
        var destinationWarehouse = await WarehouseAsync(transfer.DestinationWarehouseCode, ct);
        if (!IsGlobalRole(role) && !string.Equals(destinationWarehouse.BranchCode, userBranchCode, StringComparison.OrdinalIgnoreCase)) throw new ForbiddenAppException("You can only receive deliveries for your assigned branch.");
        if (request.Lines.Count != dispatch.Lines.Count) throw new BadRequestException("Every dispatched line must be received or reported as short/damaged.");
        var now = DateTime.UtcNow; var receipt = new StockTransferReceipt { ReceiptNo = $"TRN{now:yyyyMMddHHmmssfff}", DispatchId = dispatchId, ReceivedBy = userCode, ReceivedAt = now, Remarks = Clean(request.Remarks) }; context.StockTransferReceipts.Add(receipt);
        foreach (var dispatchLine in dispatch.Lines)
        {
            var dto = request.Lines.SingleOrDefault(x => x.DispatchLineId == dispatchLine.DispatchLineId) ?? throw new BadRequestException("Every receipt line must match a dispatched line.");
            if (dto.ReceivedQty < 0 || dto.DamagedQty < 0 || dto.ShortQty < 0 || dto.ReceivedQty + dto.DamagedQty + dto.ShortQty != dispatchLine.Quantity) throw new BadRequestException("Received, damaged and short quantities must exactly equal the dispatched quantity.");
            var sourceBatch = dispatchLine.StockBatch!; var sourceStock = sourceBatch.StockInventory!; var itemCode = sourceStock.ItemCode;
            var destStock = await context.StockInventories.SingleOrDefaultAsync(x => x.ItemCode == itemCode && x.WarehouseCode == transfer.DestinationWarehouseCode, ct);
            if (destStock is null) { destStock = new StockInventory { ItemCode = itemCode, BranchCode = destinationWarehouse.BranchCode, WarehouseCode = transfer.DestinationWarehouseCode, CurrentQty = 0, LastUpdated = now }; context.StockInventories.Add(destStock); await context.SaveChangesAsync(ct); }
            if (dto.ReceivedQty > 0)
            {
                var destBatch = await context.StockBatches.SingleOrDefaultAsync(x => x.StockId == destStock.StockId && x.BatchNo == sourceBatch.BatchNo, ct);
                var previous = destStock.CurrentQty; destStock.CurrentQty += dto.ReceivedQty; destStock.LastUpdated = now;
                if (destBatch is null) { destBatch = new StockBatch { StockId = destStock.StockId, BatchNo = sourceBatch.BatchNo, ReceivedQty = dto.ReceivedQty, AvailableQty = dto.ReceivedQty, UnitCost = dispatchLine.UnitCost, SellingPrice = sourceBatch.SellingPrice, ExpiryDate = sourceBatch.ExpiryDate, ReceivedDate = now, Status = BatchStatus.Available }; context.StockBatches.Add(destBatch); }
                else { destBatch.ReceivedQty += dto.ReceivedQty; destBatch.AvailableQty += dto.ReceivedQty; destBatch.Status = BatchStatus.Available; }
                context.StockMovements.Add(new StockMovement { StockBatch = destBatch, StockId = destStock.StockId, MovementType = StockMovementType.In, ReferenceType = StockReferenceType.StockTransfer, ReferenceNo = receipt.ReceiptNo, Qty = dto.ReceivedQty, PreviousQty = previous, NewQty = destStock.CurrentQty, UnitCost = dispatchLine.UnitCost, Remarks = $"Received from {transfer.SourceWarehouseCode}", CreatedAt = now, CreatedBy = userCode });
                context.ItemLogs.Add(ItemLogFactory.Create(itemCode, ItemLogActions.StockChanged, previous.ToString(), destStock.CurrentQty.ToString(), userCode));
            }
            var requestLine = transfer.Lines.Single(x => x.TransferRequestLineId == dispatchLine.TransferRequestLineId); requestLine.ReceivedQty += dto.ReceivedQty;
            receipt.Lines.Add(new StockTransferReceiptLine { DispatchLineId = dispatchLine.DispatchLineId, ReceivedQty = dto.ReceivedQty, DamagedQty = dto.DamagedQty, ShortQty = dto.ShortQty, Remarks = Clean(dto.Remarks) });
        }
        transfer.Status = StockTransferStatus.Received; transfer.UpdatedAt = now;
        // SaveChanges creates its own transaction; compatible with the configured retry strategy.
        await context.SaveChangesAsync(ct);
        return new StockTransferReceiptDto { ReceiptId = receipt.ReceiptId, ReceiptNo = receipt.ReceiptNo, DispatchId = dispatchId, ReceivedAt = receipt.ReceivedAt };
    }

    public async Task<(string FileName, byte[] Content)> GetDeliveryNotePdfAsync(long dispatchId, CancellationToken ct = default)
    {
        var dispatch = await context.StockTransferDispatches.AsNoTracking().Include(x => x.TransferRequest).ThenInclude(x => x!.SourceWarehouse).Include(x => x.TransferRequest).ThenInclude(x => x!.DestinationWarehouse).Include(x => x.Lines).ThenInclude(x => x.TransferRequestLine).Include(x => x.Lines).ThenInclude(x => x.StockBatch).SingleOrDefaultAsync(x => x.DispatchId == dispatchId, ct) ?? throw new NotFoundException("StockTransferDispatch", dispatchId);
        var pdf = Document.Create(document => document.Page(page => { page.Margin(25); page.Header().Text("DELIVERY NOTE").FontSize(18).Bold(); page.Content().Column(column => { column.Spacing(8); column.Item().Text($"Dispatch: {dispatch.DispatchNo}    Date: {dispatch.DispatchedAt:yyyy-MM-dd HH:mm}"); column.Item().Text($"From: {dispatch.TransferRequest!.SourceWarehouse!.WarehouseName} ({dispatch.TransferRequest.SourceWarehouseCode})"); column.Item().Text($"To: {dispatch.TransferRequest.DestinationWarehouse!.WarehouseName} ({dispatch.TransferRequest.DestinationWarehouseCode})"); column.Item().Text($"Vehicle: {dispatch.VehicleNo ?? "-"}    Driver: {dispatch.DriverName ?? "-"}"); column.Item().Table(table => { table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(); }); table.Header(h => { h.Cell().Text("Item").Bold(); h.Cell().Text("Batch").Bold(); h.Cell().Text("Qty").Bold(); h.Cell().Text("Cost").Bold(); }); foreach (var line in dispatch.Lines) { table.Cell().Text(line.TransferRequestLine!.ItemCode); table.Cell().Text(line.StockBatch!.BatchNo); table.Cell().Text(line.Quantity.ToString("0.###")); table.Cell().Text(line.UnitCost.ToString("0.00")); } }); column.Item().PaddingTop(30).Text("Prepared by: __________________   Driver: __________________   Branch receiver: __________________"); }); page.Footer().AlignCenter().Text($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8); })).GeneratePdf();
        return ($"{dispatch.DispatchNo}-DeliveryNote.pdf", pdf);
    }

    public async Task<(string FileName, byte[] Content)> GetReceiptPdfAsync(long receiptId, CancellationToken ct = default)
    {
        var receipt = await context.StockTransferReceipts.AsNoTracking().Include(x => x.Dispatch).ThenInclude(x => x!.TransferRequest).ThenInclude(x => x!.SourceWarehouse).Include(x => x.Dispatch).ThenInclude(x => x!.TransferRequest).ThenInclude(x => x!.DestinationWarehouse).Include(x => x.Lines).ThenInclude(x => x.DispatchLine).ThenInclude(x => x!.TransferRequestLine).Include(x => x.Lines).ThenInclude(x => x.DispatchLine).ThenInclude(x => x!.StockBatch).SingleOrDefaultAsync(x => x.ReceiptId == receiptId, ct) ?? throw new NotFoundException("StockTransferReceipt", receiptId);
        var pdf = Document.Create(document => document.Page(page => { page.Margin(25); page.Header().Text("BRANCH TRANSFER RECEIPT").FontSize(18).Bold(); page.Content().Column(column => { column.Spacing(8); column.Item().Text($"Receipt: {receipt.ReceiptNo}    Delivery Note: {receipt.Dispatch!.DispatchNo}"); column.Item().Text($"From: {receipt.Dispatch.TransferRequest!.SourceWarehouse!.WarehouseName}    To: {receipt.Dispatch.TransferRequest.DestinationWarehouse!.WarehouseName}"); column.Item().Text($"Received: {receipt.ReceivedAt:yyyy-MM-dd HH:mm}    By: {receipt.ReceivedBy}"); column.Item().Table(table => { table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); }); table.Header(h => { h.Cell().Text("Item").Bold(); h.Cell().Text("Batch").Bold(); h.Cell().Text("Received").Bold(); h.Cell().Text("Damaged").Bold(); h.Cell().Text("Short").Bold(); }); foreach (var line in receipt.Lines) { table.Cell().Text(line.DispatchLine!.TransferRequestLine!.ItemCode); table.Cell().Text(line.DispatchLine.StockBatch!.BatchNo); table.Cell().Text(line.ReceivedQty.ToString("0.###")); table.Cell().Text(line.DamagedQty.ToString("0.###")); table.Cell().Text(line.ShortQty.ToString("0.###")); } }); }); })).GeneratePdf();
        return ($"{receipt.ReceiptNo}-TransferReceipt.pdf", pdf);
    }

    private async Task<Warehouse> WarehouseAsync(string code, CancellationToken ct) => await context.Warehouses.SingleOrDefaultAsync(x => x.WarehouseCode == code.Trim(), ct) ?? throw new NotFoundException("Warehouse", code);
    private async Task<string> GenerateNextPoNoAsync(CancellationToken ct)
    {
        var lastCode = await context.PurchaseOrders.AsNoTracking()
            .Where(x => x.PoNo.StartsWith("PO"))
            .OrderByDescending(x => x.PoNo)
            .Select(x => x.PoNo)
            .FirstOrDefaultAsync(ct);
        var next = lastCode is not null && int.TryParse(lastCode[2..], out var current) ? current + 1 : 1;
        return $"PO{next:D6}";
    }
    private async Task<StockTransferRequest> RequestAsync(long id, CancellationToken ct) => await context.StockTransferRequests.Include(x => x.Lines).Include(x => x.SourceWarehouse).Include(x => x.DestinationWarehouse).SingleOrDefaultAsync(x => x.TransferRequestId == id, ct) ?? throw new NotFoundException("StockTransferRequest", id);
    private static bool IsGlobalRole(string? role) => string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);
    private static void EnsureWarehouseAccess(string sourceWarehouseCode, string? userWarehouseCode, string? role)
    {
        if (IsGlobalRole(role)) return;
        if (!string.Equals(role, "InventoryClerk", StringComparison.OrdinalIgnoreCase)) throw new ForbiddenAppException("Only Main Warehouse staff can process stock requests.");
        if (string.IsNullOrWhiteSpace(userWarehouseCode) || !string.Equals(sourceWarehouseCode, userWarehouseCode, StringComparison.OrdinalIgnoreCase)) throw new ForbiddenAppException("You can only process requests assigned to your warehouse.");
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static StockTransferRequestDto ToDto(StockTransferRequest x) => new() { TransferRequestId = x.TransferRequestId, RequestNo = x.RequestNo, SourceWarehouseCode = x.SourceWarehouseCode, SourceWarehouseName = x.SourceWarehouse?.WarehouseName, DestinationWarehouseCode = x.DestinationWarehouseCode, DestinationWarehouseName = x.DestinationWarehouse?.WarehouseName, Status = x.Status, RequestDate = x.RequestDate, RequiredDate = x.RequiredDate, Remarks = x.Remarks, Lines = x.Lines.Select(l => new StockTransferRequestLineDto { TransferRequestLineId = l.TransferRequestLineId, ItemCode = l.ItemCode, RequestedQty = l.RequestedQty, ApprovedQty = l.ApprovedQty, DispatchedQty = l.DispatchedQty, ReceivedQty = l.ReceivedQty, Remarks = l.Remarks }).ToList(), Dispatches = x.Dispatches.Select(ToDto).OrderByDescending(d => d.DispatchedAt).ToList() };
    private static StockTransferDispatchDto ToDto(StockTransferDispatch x) => new() { DispatchId = x.DispatchId, DispatchNo = x.DispatchNo, TransferRequestId = x.TransferRequestId, DispatchedAt = x.DispatchedAt, VehicleNo = x.VehicleNo, DriverName = x.DriverName, Lines = x.Lines.Select(l => new StockTransferDispatchLineDto { DispatchLineId = l.DispatchLineId, TransferRequestLineId = l.TransferRequestLineId, ItemCode = l.TransferRequestLine?.ItemCode ?? string.Empty, BatchId = l.BatchId, BatchNo = l.StockBatch?.BatchNo, Quantity = l.Quantity, UnitCost = l.UnitCost, SellingPrice = l.StockBatch?.SellingPrice ?? 0, ExpiryDate = l.StockBatch?.ExpiryDate }).ToList() };
}
