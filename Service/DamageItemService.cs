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

public class DamageItemService : IDamageItemService
{
    private const string DamageExpenseCategoryName = "Damaged Stock";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DamageItemService> _logger;

    public DamageItemService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DamageItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DamageItemDto>> SearchAsync(
        string? itemCode,
        string? branchCode,
        string? warehouseCode,
        DamageItemStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var damageItems = await _unitOfWork.DamageItems.SearchAsync(itemCode, branchCode, warehouseCode, status, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<DamageItemDto>>(damageItems);
    }

    public async Task<DamageItemDto> GetByIdAsync(int damageId, CancellationToken cancellationToken = default)
    {
        var damageItem = await _unitOfWork.DamageItems.GetByIdWithDetailsAsync(damageId, cancellationToken)
            ?? throw new NotFoundException("DamageItem", damageId);

        return _mapper.Map<DamageItemDto>(damageItem);
    }

    public async Task<DamageItemDto> CreateAsync(CreateDamageItemDto request, string reportedBy, string? currentWarehouseCode, string? currentRole, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
        {
            throw new BadRequestException("Damage quantity must be greater than zero.");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ItemCode, cancellationToken)
            ?? throw new NotFoundException("Product", request.ItemCode);

        if (string.IsNullOrWhiteSpace(request.WarehouseCode)) throw new BadRequestException("Warehouse code is required for a damage report.");
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.WarehouseCode, cancellationToken)
            ?? throw new NotFoundException("Warehouse", request.WarehouseCode);
        Branch? branch = null;
        if (warehouse.IsCentralWarehouse)
        {
            if (!string.IsNullOrWhiteSpace(request.BranchCode)) throw new BadRequestException("Central warehouse damage reports must not include a branch code.");
            var isGlobal = string.Equals(currentRole, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(currentRole, "Manager", StringComparison.OrdinalIgnoreCase);
            if (!isGlobal && (!string.Equals(currentRole, "InventoryClerk", StringComparison.OrdinalIgnoreCase) || !string.Equals(currentWarehouseCode, warehouse.WarehouseCode, StringComparison.OrdinalIgnoreCase)))
                throw new ForbiddenAppException("Only the InventoryClerk assigned to this central warehouse can record its damage.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.BranchCode)) throw new BadRequestException("Branch code is required for a branch warehouse damage report.");
            branch = await _unitOfWork.Branches.GetByIdAsync(request.BranchCode, cancellationToken) ?? throw new NotFoundException("Branch", request.BranchCode);
            if (!string.Equals(warehouse.BranchCode, branch.BranchCode, StringComparison.OrdinalIgnoreCase)) throw new BadRequestException("The selected branch does not own the selected warehouse.");
        }

        // ---- There is no approval workflow: locate the stock to draw down from up front (FIFO
        // across batches, same approach used to post a sale) so we fail fast if there isn't
        // enough on hand, before anything is written. ----
        var batches = (await _unitOfWork.StockBatches.GetAvailableBatchesByItemAndWarehouseAsync(product.ItemCode, warehouse.WarehouseCode, cancellationToken)).ToList();

        var availableQty = batches.Sum(b => b.AvailableQty);
        if (availableQty < request.Quantity)
        {
            throw new BadRequestException(
                $"Insufficient stock for item '{product.ItemCode}' at warehouse '{warehouse.WarehouseCode}': requested {request.Quantity}, only {availableQty} available.");
        }

        var remainingForCost = request.Quantity;
        var fifoCost = 0m;
        foreach (var batch in batches)
        {
            if (remainingForCost <= 0) break;
            var take = Math.Min(remainingForCost, batch.AvailableQty);
            fifoCost += take * batch.UnitCost;
            remainingForCost -= take;
        }
        var damageCost = request.CostAmount ?? fifoCost;
        if (damageCost < 0)
        {
            throw new BadRequestException("Damage cost cannot be negative.");
        }

        // ---- Insert damage_item first so subsequent stock_movement rows have a DamageId to
        // reference. ----
        var damageItem = new DamageItem
        {
            ItemCode = product.ItemCode,
            BranchCode = warehouse.IsCentralWarehouse ? null : branch!.BranchCode,
            WarehouseCode = warehouse.WarehouseCode,
            Quantity = request.Quantity,
            CostAmount = damageCost,
            Reason = request.Reason,
            DamageDate = request.DamageDate ?? DateTime.UtcNow,
            ReportedBy = reportedBy,
            Status = DamageItemStatus.Reported
        };

        await _unitOfWork.DamageItems.AddAsync(damageItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var referenceNo = $"DMG-{damageItem.DamageId}";

        // ---- Posted immediately: update stock_batch / stock_inventory and insert a
        // stock_movement ("Out") for every batch drawn from. ----
        var remaining = request.Quantity;
        decimal stockQtyBeforeFirstBatch = 0;
        decimal stockQtyAfterLastBatch = 0;
        var isFirstBatch = true;

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
                batch.Status = BatchStatus.Damaged;
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

            if (isFirstBatch)
            {
                stockQtyBeforeFirstBatch = previousQty;
                isFirstBatch = false;
            }
            stockQtyAfterLastBatch = stock.CurrentQty;

            await _unitOfWork.StockMovements.AddAsync(new StockMovement
            {
                StockInventory = stock,
                StockBatch = batch,
                MovementType = StockMovementType.Out,
                ReferenceType = StockReferenceType.Damage,
                ReferenceNo = referenceNo,
                Qty = -take,
                PreviousQty = previousQty,
                NewQty = stock.CurrentQty,
                UnitCost = batch.UnitCost,
                Remarks = $"Damage report {referenceNo} - item {product.ItemCode} written off from batch {batch.BatchNo}.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = reportedBy
            }, cancellationToken);

            remaining -= take;
        }

        // ---- item_log: automatic audit entry for the damage post. ----
        await _unitOfWork.ItemLogs.AddAsync(
            ItemLogFactory.Create(
                product.ItemCode,
                ItemLogActions.Damage,
                stockQtyBeforeFirstBatch.ToString(),
                stockQtyAfterLastBatch.ToString(),
                reportedBy),
            cancellationToken);

        // Central warehouse write-offs are inventory-only. They must never create Expenses.
        if (!warehouse.IsCentralWarehouse) await CreateDamageExpenseAsync(damageItem, reportedBy, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Damage report {DamageId} posted for item {ItemCode} at branch {BranchCode}: {Quantity} unit(s) written off by {ReportedBy}",
            damageItem.DamageId, product.ItemCode, warehouse.WarehouseCode, request.Quantity, reportedBy);

        return await GetByIdAsync(damageItem.DamageId, cancellationToken);
    }

    public async Task<DamageItemDto> UpdateAsync(int damageId, UpdateDamageItemDto request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
        {
            throw new BadRequestException("Damage quantity must be greater than zero.");
        }

        var damageItem = await _unitOfWork.DamageItems.GetByIdAsync(damageId, cancellationToken)
            ?? throw new NotFoundException("DamageItem", damageId);

        var product = await _unitOfWork.Products.GetByIdAsync(request.ItemCode, cancellationToken)
            ?? throw new NotFoundException("Product", request.ItemCode);

        if (string.IsNullOrWhiteSpace(request.WarehouseCode)) throw new BadRequestException("Warehouse code is required for a damage report.");
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(request.WarehouseCode, cancellationToken)
            ?? throw new NotFoundException("Warehouse", request.WarehouseCode);
        Branch? branch = null;
        if (warehouse.IsCentralWarehouse)
        {
            if (!string.IsNullOrWhiteSpace(request.BranchCode)) throw new BadRequestException("Central warehouse damage reports must not include a branch code.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.BranchCode)) throw new BadRequestException("Branch code is required for a branch warehouse damage report.");
            branch = await _unitOfWork.Branches.GetByIdAsync(request.BranchCode, cancellationToken) ?? throw new NotFoundException("Branch", request.BranchCode);
            if (!string.Equals(warehouse.BranchCode, branch.BranchCode, StringComparison.OrdinalIgnoreCase)) throw new BadRequestException("The selected branch does not own the selected warehouse.");
        }

        damageItem.ItemCode = product.ItemCode;
        damageItem.BranchCode = warehouse.IsCentralWarehouse ? null : branch!.BranchCode;
        damageItem.WarehouseCode = warehouse.WarehouseCode;
        damageItem.Quantity = request.Quantity;
        damageItem.CostAmount = request.CostAmount;
        damageItem.Reason = request.Reason;
        damageItem.DamageDate = request.DamageDate ?? damageItem.DamageDate;
        damageItem.Status = request.Status;

        _unitOfWork.DamageItems.Update(damageItem);
        if (!warehouse.IsCentralWarehouse) await SyncDamageExpenseAsync(damageItem, cancellationToken);
        else
        {
            var existingExpense = (await _unitOfWork.Expenses.FindAsync(x => x.DamageId == damageItem.DamageId, cancellationToken)).SingleOrDefault();
            if (existingExpense is not null) _unitOfWork.Expenses.Remove(existingExpense);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Damage report {DamageId} updated to status {Status}", damageId, request.Status);

        return await GetByIdAsync(damageId, cancellationToken);
    }

    public async Task DeleteAsync(int damageId, CancellationToken cancellationToken = default)
    {
        var damageItem = await _unitOfWork.DamageItems.GetByIdAsync(damageId, cancellationToken)
            ?? throw new NotFoundException("DamageItem", damageId);

        // Since a damage report posts immediately with no approval step, deleting it must
        // reverse what it posted: restore the stock_movement "Out" entries it raised.
        var referenceNo = $"DMG-{damageId}";
        var movements = await _unitOfWork.StockMovements.FindAsync(m => m.ReferenceNo == referenceNo, cancellationToken);

        foreach (var movement in movements)
        {
            var batch = await _unitOfWork.StockBatches.GetByIdAsync(movement.BatchId, cancellationToken);
            if (batch is not null)
            {
                batch.AvailableQty -= movement.Qty; // Qty is negative for the original write-off, so this adds it back.
                if (batch.Status == BatchStatus.Damaged && batch.AvailableQty > 0)
                {
                    batch.Status = BatchStatus.Available;
                }
                _unitOfWork.StockBatches.Update(batch);
            }

            var stock = await _unitOfWork.StockInventories.GetByIdAsync(movement.StockId, cancellationToken);
            if (stock is not null)
            {
                stock.CurrentQty -= movement.Qty;
                stock.LastUpdated = DateTime.UtcNow;
                _unitOfWork.StockInventories.Update(stock);
            }

            _unitOfWork.StockMovements.Remove(movement);
        }

        _unitOfWork.DamageItems.Remove(damageItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Damage report {DamageId} deleted and reversed successfully", damageId);
    }

    private async Task CreateDamageExpenseAsync(
        DamageItem damageItem,
        string paidBy,
        CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.ExpenseCategories.FindAsync(
            x => x.CategoryName == DamageExpenseCategoryName,
            cancellationToken);
        var category = categories.FirstOrDefault();
        if (category is null)
        {
            category = new ExpenseCategory
            {
                CategoryName = DamageExpenseCategoryName,
                Description = "Automatically generated expenses for damaged stock."
            };
            await _unitOfWork.ExpenseCategories.AddAsync(category, cancellationToken);
        }

        var isNewCategory = category.CategoryId == 0;
        await _unitOfWork.Expenses.AddAsync(new Expense
        {
            DamageId = damageItem.DamageId,
            BranchCode = damageItem.BranchCode,
            CategoryId = isNewCategory ? null : category.CategoryId,
            Category = isNewCategory ? category : null,
            Amount = damageItem.CostAmount ?? 0,
            ExpenseDate = DateOnly.FromDateTime(damageItem.DamageDate ?? DateTime.UtcNow),
            Description = $"Damage DMG-{damageItem.DamageId}: {damageItem.ItemCode} - {damageItem.Reason}",
            PaidBy = paidBy,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task SyncDamageExpenseAsync(
        DamageItem damageItem,
        CancellationToken cancellationToken)
    {
        var expenses = await _unitOfWork.Expenses.FindAsync(
            x => x.DamageId == damageItem.DamageId,
            cancellationToken);
        var expense = expenses.SingleOrDefault();

        if (expense is null)
        {
            await CreateDamageExpenseAsync(damageItem, damageItem.ReportedBy ?? string.Empty, cancellationToken);
            return;
        }

        expense.BranchCode = damageItem.BranchCode;
        expense.Amount = damageItem.CostAmount ?? 0;
        expense.ExpenseDate = DateOnly.FromDateTime(damageItem.DamageDate ?? DateTime.UtcNow);
        expense.Description = $"Damage DMG-{damageItem.DamageId}: {damageItem.ItemCode} - {damageItem.Reason}";
        _unitOfWork.Expenses.Update(expense);
    }
}
