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

public class StockMovementService : IStockMovementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<StockMovementService> _logger;

    public StockMovementService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StockMovementService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StockMovementDto>> SearchAsync(
        int? stockId,
        long? batchId,
        string? referenceNo,
        CancellationToken cancellationToken = default)
    {
        var movements = await _unitOfWork.StockMovements.SearchAsync(stockId, batchId, referenceNo, cancellationToken);
        return _mapper.Map<IReadOnlyList<StockMovementDto>>(movements);
    }

    public async Task<StockMovementDto> GetByIdAsync(long movementId, CancellationToken cancellationToken = default)
    {
        var movement = await _unitOfWork.StockMovements.GetByIdAsync(movementId, cancellationToken)
            ?? throw new NotFoundException("StockMovement", movementId);

        return _mapper.Map<StockMovementDto>(movement);
    }

    public async Task<StockMovementDto> CreateAsync(CreateStockMovementDto request, string createdBy, CancellationToken cancellationToken = default)
    {
        var stock = await _unitOfWork.StockInventories.GetByIdAsync(request.StockId, cancellationToken)
            ?? throw new NotFoundException("StockInventory", request.StockId);

        var batch = await _unitOfWork.StockBatches.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException("StockBatch", request.BatchId);

        if (batch.StockId != request.StockId)
        {
            throw new BadRequestException($"Batch {request.BatchId} does not belong to stock line {request.StockId}.");
        }

        if (batch.Status != BatchStatus.Available)
        {
            throw new ConflictException($"Batch '{batch.BatchNo}' is not Available (current status: {batch.Status}) and cannot be moved.");
        }

        var previousQty = batch.AvailableQty;
        var newQty = previousQty + request.Qty;

        if (newQty < 0)
        {
            throw new BadRequestException(
                $"Insufficient available quantity in batch '{batch.BatchNo}': requested {-request.Qty}, only {previousQty} available.");
        }

        var stockNewQty = stock.CurrentQty + request.Qty;
        if (stockNewQty < 0)
        {
            throw new BadRequestException($"Insufficient available quantity on stock line {stock.StockId}.");
        }

        batch.AvailableQty = newQty;
        _unitOfWork.StockBatches.Update(batch);

        var stockPreviousQty = stock.CurrentQty;
        stock.CurrentQty = stockNewQty;
        stock.LastUpdated = DateTime.UtcNow;
        _unitOfWork.StockInventories.Update(stock);

        var movement = new StockMovement
        {
            StockId = request.StockId,
            BatchId = request.BatchId,
            MovementType = request.MovementType,
            ReferenceType = request.ReferenceType,
            ReferenceNo = request.ReferenceNo,
            Qty = request.Qty,
            PreviousQty = previousQty,
            NewQty = newQty,
            UnitCost = request.UnitCost ?? batch.UnitCost,
            Remarks = request.Remarks,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.StockMovements.AddAsync(movement, cancellationToken);

        await _unitOfWork.ItemLogs.AddAsync(
            ItemLogFactory.Create(stock.ItemCode, ItemLogActions.StockChanged, stockPreviousQty.ToString(), stock.CurrentQty.ToString(), createdBy),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Movement recorded for batch {BatchNo}: {Qty:+0.###;-0.###} (batch qty {Previous} -> {New}, stock qty {StockPrevious} -> {StockNew})",
            batch.BatchNo, request.Qty, previousQty, newQty, stockPreviousQty, stock.CurrentQty);

        return _mapper.Map<StockMovementDto>(movement);
    }

    public async Task<StockMovementDto> UpdateAsync(long movementId, UpdateStockMovementDto request, CancellationToken cancellationToken = default)
    {
        var movement = await _unitOfWork.StockMovements.GetByIdAsync(movementId, cancellationToken)
            ?? throw new NotFoundException("StockMovement", movementId);

        // Quantities and timestamps are never editable here - movements are an immutable audit
        // trail. Only descriptive metadata can be corrected.
        movement.ReferenceNo = request.ReferenceNo;
        movement.Remarks = request.Remarks;

        _unitOfWork.StockMovements.Update(movement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StockMovementDto>(movement);
    }

    public async Task DeleteAsync(long movementId, CancellationToken cancellationToken = default)
    {
        var movement = await _unitOfWork.StockMovements.GetByIdAsync(movementId, cancellationToken)
            ?? throw new NotFoundException("StockMovement", movementId);

        if (movement.MovementType != StockMovementType.Adjustment)
        {
            throw new ConflictException(
                "Only manually recorded adjustment movements can be deleted; system-generated entries (receipts, sales, etc) are immutable.");
        }

        var latest = await _unitOfWork.StockMovements.GetLatestForBatchAsync(movement.BatchId, cancellationToken);
        if (latest is null || latest.MovementId != movement.MovementId)
        {
            throw new ConflictException(
                "Only the most recently recorded movement for a batch can be deleted, to preserve the audit trail. Record a reversing adjustment instead.");
        }

        var batch = await _unitOfWork.StockBatches.GetByIdAsync(movement.BatchId, cancellationToken)
            ?? throw new NotFoundException("StockBatch", movement.BatchId);

        var stock = await _unitOfWork.StockInventories.GetByIdAsync(movement.StockId, cancellationToken)
            ?? throw new NotFoundException("StockInventory", movement.StockId);

        batch.AvailableQty -= movement.Qty;
        if (batch.AvailableQty < 0)
        {
            batch.AvailableQty = 0;
        }

        stock.CurrentQty -= movement.Qty;
        if (stock.CurrentQty < 0)
        {
            stock.CurrentQty = 0;
        }
        stock.LastUpdated = DateTime.UtcNow;

        _unitOfWork.StockBatches.Update(batch);
        _unitOfWork.StockInventories.Update(stock);
        _unitOfWork.StockMovements.Remove(movement);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Movement {MovementId} deleted and reversed for batch {BatchId}", movementId, movement.BatchId);
    }
}
