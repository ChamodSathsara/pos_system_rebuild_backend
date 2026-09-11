using PosApi.DTOs.Stock;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class OpeningStockService : IOpeningStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockBatchService _stockBatchService;
    private readonly ILogger<OpeningStockService> _logger;

    public OpeningStockService(
        IUnitOfWork unitOfWork,
        IStockBatchService stockBatchService,
        ILogger<OpeningStockService> logger)
    {
        _unitOfWork = unitOfWork;
        _stockBatchService = stockBatchService;
        _logger = logger;
    }

    public async Task<OpeningStockDto> CreateAsync(
        CreateOpeningStockDto request,
        string createdBy,
        string? userBranchCode,
        string? userWarehouseCode,
        string? userRole,
        CancellationToken cancellationToken = default)
    {
        var itemCode = request.ItemCode.Trim();
        var branchCode = string.IsNullOrWhiteSpace(request.BranchCode) ? null : request.BranchCode.Trim();
        var warehouseCode = request.WarehouseCode.Trim();
        var batchNo = await _unitOfWork.StockBatches.GenerateNextBatchNoAsync(cancellationToken);
        var openingDate = request.OpeningDate ?? DateTime.UtcNow;

        var (product, warehouse) = await ValidateReferencesAsync(
            itemCode,
            branchCode,
            warehouseCode,
            cancellationToken);

        ValidateWarehouseAccess(warehouse, branchCode, userBranchCode, userWarehouseCode, userRole);

        product.SellingPrice = request.SellingPrice;
        product.UpdatedAt = DateTime.UtcNow;

        var stock = await _unitOfWork.StockInventories
            .GetByCombinationAsync(
                itemCode,
                branchCode,
                warehouseCode,
                cancellationToken);

        if (stock is null)
        {
            stock = new StockInventory
            {
                ItemCode = itemCode,
                BranchCode = branchCode,
                WarehouseCode = warehouseCode,
                CurrentQty = 0,
                LastUpdated = DateTime.UtcNow
            };

            await _unitOfWork.StockInventories.AddAsync(
                stock,
                cancellationToken);

            // Save here to generate the StockId required by StockBatchService.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Stock line created for item {ItemCode}, branch " +
                "{BranchCode}, warehouse {WarehouseCode}. StockId: {StockId}",
                itemCode,
                branchCode,
                warehouseCode,
                stock.StockId);
        }

        var hasOpeningStock =
            await _unitOfWork.StockMovements.HasOpeningStockAsync(
                stock.StockId,
                cancellationToken);

        if (hasOpeningStock)
        {
            throw new ConflictException(
                $"Opening stock has already been applied to item " +
                $"'{itemCode}' in warehouse '{warehouseCode}'. Use the Central Stock Receipt endpoint for additional central warehouse stock.");
        }

        if (stock.CurrentQty != 0)
        {
            throw new ConflictException(
                $"The stock line already has a quantity of " +
                $"{stock.CurrentQty}. Opening stock can only be " +
                "applied when the current quantity is zero.");
        }

        var batchExists = await _unitOfWork.StockBatches
            .BatchNoExistsAsync(
                stock.StockId,
                batchNo,
                cancellationToken);

        if (batchExists)
        {
            throw new ConflictException(
                $"Batch '{batchNo}' already exists for this stock line.");
        }

        var referenceNo =
            string.IsNullOrWhiteSpace(request.ReferenceNo)
                ? $"OPEN-{itemCode}-{warehouseCode}-{openingDate:yyyyMMdd}"
                : request.ReferenceNo.Trim();

        var batchRequest = new CreateStockBatchDto
        {
            StockId = stock.StockId,
            BatchNo = batchNo,
            ReceivedQty = request.Quantity,
            UnitCost = request.UnitCost,
            SellingPrice = request.SellingPrice,
            ExpiryDate = request.ExpiryDate,
            ReceivedDate = openingDate,
            ReferenceType = StockReferenceType.OpeningStock,
            ReferenceNo = referenceNo,
            Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                ? "Opening stock entry"
                : request.Remarks.Trim()
        };

        var batch = await _stockBatchService.CreateAsync(
            batchRequest,
            createdBy,
            cancellationToken);

        _logger.LogInformation(
            "Opening stock applied to stock line {StockId}. " +
            "Item: {ItemCode}, branch: {BranchCode}, " +
            "warehouse: {WarehouseCode}, quantity: {Quantity}, " +
            "batch: {BatchNo}, user: {CreatedBy}",
            stock.StockId,
            itemCode,
            branchCode,
            warehouseCode,
            request.Quantity,
            batch.BatchNo,
            createdBy);

        return new OpeningStockDto
        {
            StockId = batch.StockId,
            BatchId = batch.BatchId,
            BatchNo = batch.BatchNo,
            Quantity = batch.ReceivedQty,
            UnitCost = batch.UnitCost,
            SellingPrice = request.SellingPrice,
            TotalValue = batch.ReceivedQty * batch.UnitCost,
            ExpiryDate = batch.ExpiryDate,
            OpeningDate = batch.ReceivedDate,
            ReferenceNo = referenceNo,
            ReferenceType = StockReferenceType.OpeningStock
        };
    }

    private async Task<(ProductMaster Product, Warehouse Warehouse)> ValidateReferencesAsync(
        string itemCode,
        string? branchCode,
        string warehouseCode,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(
            itemCode,
            cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(
                "ProductMaster",
                itemCode);
        }

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(
            warehouseCode,
            cancellationToken);

        if (warehouse is null)
        {
            throw new NotFoundException(
                "Warehouse",
                warehouseCode);
        }

        if (warehouse.IsCentralWarehouse)
        {
            if (!string.IsNullOrWhiteSpace(branchCode)) throw new ConflictException("Central warehouse opening stock must not include a branch code.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(branchCode)) throw new ConflictException("Branch code is required for a branch warehouse.");
            var branch = await _unitOfWork.Branches.GetByIdAsync(branchCode, cancellationToken) ?? throw new NotFoundException("Branch", branchCode);
            if (!string.Equals(warehouse.BranchCode, branch.BranchCode, StringComparison.OrdinalIgnoreCase)) throw new ConflictException($"Warehouse '{warehouseCode}' does not belong to branch '{branchCode}'.");
        }
        return (product, warehouse);
    }

    private static void ValidateWarehouseAccess(
        Warehouse warehouse,
        string? requestedBranchCode,
        string? userBranchCode,
        string? userWarehouseCode,
        string? userRole)
    {
        var isHeadOfficeUser =
            string.Equals(
                userRole,
                "Admin",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                userRole,
                "Manager",
                StringComparison.OrdinalIgnoreCase);

        if (isHeadOfficeUser)
        {
            return;
        }

        if (warehouse.IsCentralWarehouse)
        {
            if (!string.Equals(userRole, "InventoryClerk", StringComparison.OrdinalIgnoreCase) || !string.Equals(userWarehouseCode, warehouse.WarehouseCode, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Only the InventoryClerk assigned to this central warehouse can add its stock.");
            return;
        }
        if (string.IsNullOrWhiteSpace(userBranchCode) || !string.Equals(requestedBranchCode, userBranchCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You cannot apply opening stock to another branch.");
        }
    }
}
