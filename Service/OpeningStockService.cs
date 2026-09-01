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
        string? userRole,
        CancellationToken cancellationToken = default)
    {
        var itemCode = request.ItemCode.Trim();
        var branchCode = request.BranchCode.Trim();
        var warehouseCode = request.WarehouseCode.Trim();
        var batchNo = request.BatchNo.Trim();
        var openingDate = request.OpeningDate ?? DateTime.UtcNow;

        ValidateBranchAccess(
            branchCode,
            userBranchCode,
            userRole);

        await ValidateReferencesAsync(
            itemCode,
            branchCode,
            warehouseCode,
            cancellationToken);

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
                $"'{itemCode}' in branch '{branchCode}' and " +
                $"warehouse '{warehouseCode}'.");
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
                ? $"OPEN-{itemCode}-{branchCode}-{openingDate:yyyyMMdd}"
                : request.ReferenceNo.Trim();

        var batchRequest = new CreateStockBatchDto
        {
            StockId = stock.StockId,
            BatchNo = batchNo,
            ReceivedQty = request.Quantity,
            UnitCost = request.UnitCost,
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
            TotalValue = batch.ReceivedQty * batch.UnitCost,
            ExpiryDate = batch.ExpiryDate,
            OpeningDate = batch.ReceivedDate,
            ReferenceNo = referenceNo,
            ReferenceType = StockReferenceType.OpeningStock
        };
    }

    private async Task ValidateReferencesAsync(
        string itemCode,
        string branchCode,
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

        var branch = await _unitOfWork.Branches.GetByIdAsync(
            branchCode,
            cancellationToken);

        if (branch is null)
        {
            throw new NotFoundException(
                "Branch",
                branchCode);
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

        // Warehouse එක request කළ branch එකට අයිතිද කියලා check කරනවා.
        if (!string.Equals(
                warehouse.BranchCode,
                branchCode,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                $"Warehouse '{warehouseCode}' does not belong " +
                $"to branch '{branchCode}'.");
        }
    }

    private static void ValidateBranchAccess(
        string requestedBranchCode,
        string? userBranchCode,
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

        if (string.IsNullOrWhiteSpace(userBranchCode))
        {
            throw new UnauthorizedAccessException(
                "The current user is not assigned to a branch.");
        }

        if (!string.Equals(
                requestedBranchCode,
                userBranchCode,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "You cannot apply opening stock to another branch.");
        }
    }
}