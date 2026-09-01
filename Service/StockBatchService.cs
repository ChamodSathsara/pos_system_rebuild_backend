using AutoMapper;
using PosApi.Constants;
using PosApi.DTOs.Stock;
using PosApi.Exceptions;
using PosApi.Helpers;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class StockBatchService : IStockBatchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<StockBatchService> _logger;

    public StockBatchService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StockBatchService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StockBatchDto>> GetByStockIdAsync(int stockId, CancellationToken cancellationToken = default)
    {
        var batches = await _unitOfWork.StockBatches.GetByStockIdAsync(stockId, cancellationToken);
        return _mapper.Map<IReadOnlyList<StockBatchDto>>(batches);
    }

    public async Task<StockBatchDto> GetByIdAsync(long batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _unitOfWork.StockBatches.GetByIdAsync(batchId, cancellationToken)
            ?? throw new NotFoundException("StockBatch", batchId);

        return _mapper.Map<StockBatchDto>(batch);
    }

    public async Task<StockBatchDto> CreateAsync(CreateStockBatchDto request, string createdBy, CancellationToken cancellationToken = default)
    {
        var stock = await _unitOfWork.StockInventories.GetByIdAsync(request.StockId, cancellationToken)
            ?? throw new NotFoundException("StockInventory", request.StockId);

        var batchNo = request.BatchNo.Trim();

        if (await _unitOfWork.StockBatches.BatchNoExistsAsync(request.StockId, batchNo, cancellationToken))
        {
            throw new ConflictException($"Batch '{batchNo}' already exists for stock line {request.StockId}.");
        }

        var previousQty = stock.CurrentQty;
        var newQty = previousQty + request.ReceivedQty;

        var batch = new StockBatch
        {
            StockId = request.StockId,
            BatchNo = batchNo,
            ReceivedQty = request.ReceivedQty,
            AvailableQty = request.ReceivedQty,
            UnitCost = request.UnitCost,
            ExpiryDate = request.ExpiryDate,
            ReceivedDate = request.ReceivedDate ?? DateTime.UtcNow,
            Status = BatchStatus.Available
        };

        await _unitOfWork.StockBatches.AddAsync(batch, cancellationToken);

        stock.CurrentQty = newQty;
        stock.LastUpdated = DateTime.UtcNow;
        _unitOfWork.StockInventories.Update(stock);

        var movement = new StockMovement
        {
            StockBatch = batch,
            StockId = stock.StockId,
            MovementType = StockMovementType.In,
            ReferenceType = request.ReferenceType,
            ReferenceNo = request.ReferenceNo,
            Qty = request.ReceivedQty,
            PreviousQty = previousQty,
            NewQty = newQty,
            UnitCost = request.UnitCost,
            Remarks = request.Remarks,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.StockMovements.AddAsync(movement, cancellationToken);

        await _unitOfWork.ItemLogs.AddAsync(
            ItemLogFactory.Create(stock.ItemCode, ItemLogActions.StockChanged, previousQty.ToString(), newQty.ToString(), createdBy),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Batch {BatchNo} received for stock line {StockId}: +{Qty} (new qty {NewQty})",
            batchNo, request.StockId, request.ReceivedQty, newQty);

        return _mapper.Map<StockBatchDto>(batch);
    }

    public async Task<StockBatchDto> UpdateAsync(long batchId, UpdateStockBatchDto request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var batch = await _unitOfWork.StockBatches.GetByIdAsync(batchId, cancellationToken)
            ?? throw new NotFoundException("StockBatch", batchId);

        var newBatchNo = request.BatchNo.Trim();
        if (!string.Equals(newBatchNo, batch.BatchNo, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await _unitOfWork.StockBatches.ExistsAsync(
                b => b.StockId == batch.StockId && b.BatchNo == newBatchNo && b.BatchId != batchId,
                cancellationToken);

            if (duplicate)
            {
                throw new ConflictException($"Batch '{newBatchNo}' already exists for stock line {batch.StockId}.");
            }
        }

        var isClosingStatus = request.Status is BatchStatus.Expired or BatchStatus.Damaged or BatchStatus.Blocked;
        var needsWriteOff = batch.Status == BatchStatus.Available && isClosingStatus && batch.AvailableQty > 0;

        if (needsWriteOff)
        {
            var stock = await _unitOfWork.StockInventories.GetByIdAsync(batch.StockId, cancellationToken)
                ?? throw new NotFoundException("StockInventory", batch.StockId);

            var writeOffQty = batch.AvailableQty;
            var previousQty = stock.CurrentQty;
            var newQty = previousQty - writeOffQty;

            stock.CurrentQty = newQty < 0 ? 0 : newQty;
            stock.LastUpdated = DateTime.UtcNow;
            _unitOfWork.StockInventories.Update(stock);

            var movement = new StockMovement
            {
                StockBatch = batch,
                StockId = batch.StockId,
                MovementType = StockMovementType.Adjustment,
                ReferenceType = request.Status == BatchStatus.Damaged ? StockReferenceType.Damage : StockReferenceType.StockAdjustment,
                Qty = -writeOffQty,
                PreviousQty = previousQty,
                NewQty = stock.CurrentQty,
                UnitCost = batch.UnitCost,
                Remarks = request.Remarks ?? $"Batch {batch.BatchNo} written off - status changed to {request.Status}.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = updatedBy
            };

            await _unitOfWork.StockMovements.AddAsync(movement, cancellationToken);

            await _unitOfWork.ItemLogs.AddAsync(
                ItemLogFactory.Create(stock.ItemCode, ItemLogActions.StockChanged, previousQty.ToString(), stock.CurrentQty.ToString(), updatedBy),
                cancellationToken);

            batch.AvailableQty = 0;

            _logger.LogInformation(
                "Batch {BatchNo} written off ({Qty} units) due to status change to {Status}",
                batch.BatchNo, writeOffQty, request.Status);
        }

        batch.BatchNo = newBatchNo;
        batch.UnitCost = request.UnitCost;
        batch.ExpiryDate = request.ExpiryDate;
        batch.Status = request.Status;

        _unitOfWork.StockBatches.Update(batch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StockBatchDto>(batch);
    }

    public async Task DeleteAsync(long batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _unitOfWork.StockBatches.GetByIdAsync(batchId, cancellationToken)
            ?? throw new NotFoundException("StockBatch", batchId);

        if (batch.AvailableQty != batch.ReceivedQty)
        {
            throw new ConflictException(
                $"Batch '{batch.BatchNo}' has already been partially consumed or adjusted and cannot be deleted.");
        }

        if (await _unitOfWork.StockBatches.HasMovementsBeyondReceiptAsync(batchId, cancellationToken))
        {
            throw new ConflictException($"Batch '{batch.BatchNo}' has movement history beyond its initial receipt and cannot be deleted.");
        }

        var stock = await _unitOfWork.StockInventories.GetByIdAsync(batch.StockId, cancellationToken)
            ?? throw new NotFoundException("StockInventory", batch.StockId);

        stock.CurrentQty -= batch.ReceivedQty;
        if (stock.CurrentQty < 0)
        {
            stock.CurrentQty = 0;
        }
        stock.LastUpdated = DateTime.UtcNow;
        _unitOfWork.StockInventories.Update(stock);

        var movements = await _unitOfWork.StockMovements.FindAsync(m => m.BatchId == batchId, cancellationToken);
        foreach (var movement in movements)
        {
            _unitOfWork.StockMovements.Remove(movement);
        }

        _unitOfWork.StockBatches.Remove(batch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Batch {BatchNo} (id {BatchId}) deleted successfully", batch.BatchNo, batchId);
    }
}
