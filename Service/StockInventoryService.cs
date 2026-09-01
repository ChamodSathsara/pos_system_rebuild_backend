using AutoMapper;
using PosApi.DTOs.Stock;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class StockInventoryService : IStockInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<StockInventoryService> _logger;

    public StockInventoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StockInventoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StockInventoryDto>> SearchAsync(
        string? itemCode,
        string? branchCode,
        string? warehouseCode,
        bool onlyBelowReorderLevel = false,
        CancellationToken cancellationToken = default)
    {
        var stock = await _unitOfWork.StockInventories.SearchAsync(itemCode, branchCode, warehouseCode, onlyBelowReorderLevel, cancellationToken);
        return _mapper.Map<IReadOnlyList<StockInventoryDto>>(stock);
    }

    public async Task<StockInventoryDto> GetByIdAsync(int stockId, CancellationToken cancellationToken = default)
    {
        var stock = await _unitOfWork.StockInventories.GetByIdAsync(stockId, cancellationToken)
            ?? throw new NotFoundException("StockInventory", stockId);

        return _mapper.Map<StockInventoryDto>(stock);
    }

    public async Task<StockInventoryDto> CreateAsync(CreateStockInventoryDto request, CancellationToken cancellationToken = default)
    {
        var itemCode = request.ItemCode.Trim();
        var branchCode = request.BranchCode.Trim();
        var warehouseCode = request.WarehouseCode.Trim();

        if (!await _unitOfWork.Products.ItemCodeExistsAsync(itemCode, cancellationToken))
        {
            throw new BadRequestException($"Product '{itemCode}' does not exist.");
        }

        if (!await _unitOfWork.Branches.BranchCodeExistsAsync(branchCode, cancellationToken))
        {
            throw new BadRequestException($"Branch '{branchCode}' does not exist.");
        }

        if (!await _unitOfWork.Warehouses.WarehouseCodeExistsAsync(warehouseCode, cancellationToken))
        {
            throw new BadRequestException($"Warehouse '{warehouseCode}' does not exist.");
        }

        if (await _unitOfWork.StockInventories.GetByCombinationAsync(itemCode, branchCode, warehouseCode, cancellationToken) is not null)
        {
            throw new ConflictException(
                $"A stock line for item '{itemCode}' at branch '{branchCode}' / warehouse '{warehouseCode}' already exists.");
        }

        var stock = new StockInventory
        {
            ItemCode = itemCode,
            BranchCode = branchCode,
            WarehouseCode = warehouseCode,
            CurrentQty = 0,
            LastUpdated = DateTime.UtcNow
        };

        await _unitOfWork.StockInventories.AddAsync(stock, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Stock line {StockId} created for item {ItemCode} at {BranchCode}/{WarehouseCode}",
            stock.StockId, itemCode, branchCode, warehouseCode);

        return _mapper.Map<StockInventoryDto>(stock);
    }

    public async Task<StockInventoryDto> ReconcileAsync(int stockId, CancellationToken cancellationToken = default)
    {
        var stock = await _unitOfWork.StockInventories.GetByIdWithBatchesAsync(stockId, cancellationToken)
            ?? throw new NotFoundException("StockInventory", stockId);

        var recountedQty = stock.Batches
            .Where(b => b.Status == BatchStatus.Available || b.Status == BatchStatus.Completed)
            .Sum(b => b.AvailableQty);

        stock.CurrentQty = recountedQty;
        stock.LastUpdated = DateTime.UtcNow;

        _unitOfWork.StockInventories.Update(stock);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Stock line {StockId} reconciled to {Qty}", stockId, recountedQty);

        return _mapper.Map<StockInventoryDto>(stock);
    }

    public async Task DeleteAsync(int stockId, CancellationToken cancellationToken = default)
    {
        var stock = await _unitOfWork.StockInventories.GetByIdWithBatchesAsync(stockId, cancellationToken)
            ?? throw new NotFoundException("StockInventory", stockId);

        if (stock.CurrentQty != 0)
        {
            throw new ConflictException($"Stock line {stockId} still has {stock.CurrentQty} units on hand and cannot be deleted.");
        }

        if (stock.Batches.Count > 0)
        {
            throw new ConflictException($"Stock line {stockId} still has batches recorded against it and cannot be deleted.");
        }

        _unitOfWork.StockInventories.Remove(stock);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Stock line {StockId} deleted successfully", stockId);
    }
}
