using AutoMapper;
using PosApi.DTOs.Purchase;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PurchaseOrderService> _logger;

    public PurchaseOrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseOrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PurchaseOrderDto>> SearchAsync(
        int? vendorId,
        string? branchCode,
        PurchaseOrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.PurchaseOrders.SearchAsync(vendorId, branchCode, status, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<PurchaseOrderDto>>(orders);
    }

    public async Task<PurchaseOrderDto> GetByIdAsync(string poNo, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(poNo, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", poNo);

        return _mapper.Map<PurchaseOrderDto>(order);
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto request, string createdBy, CancellationToken cancellationToken = default)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.VendorId, cancellationToken)
            ?? throw new NotFoundException("Vendor", request.VendorId);

        if (!vendor.IsActive)
        {
            throw new BadRequestException($"Vendor '{vendor.VendorCode}' is inactive and cannot receive new purchase orders.");
        }

        if (await _unitOfWork.Branches.GetByIdAsync(request.BranchCode, cancellationToken) is null)
        {
            throw new BadRequestException($"Branch '{request.BranchCode}' does not exist.");
        }

        var poNo = request.PoNo?.Trim();
        if (string.IsNullOrWhiteSpace(poNo))
        {
            poNo = await _unitOfWork.PurchaseOrders.GenerateNextPoNoAsync(cancellationToken);
        }
        else if (await _unitOfWork.PurchaseOrders.PoNoExistsAsync(poNo, cancellationToken))
        {
            throw new ConflictException($"A purchase order with number '{poNo}' already exists.");
        }

        var items = new List<PurchaseOrderItem>();
        decimal totalAmount = 0;

        foreach (var line in request.Items)
        {
            if (await _unitOfWork.Products.GetByIdAsync(line.ItemCode, cancellationToken) is null)
            {
                throw new BadRequestException($"Product '{line.ItemCode}' does not exist.");
            }

            var totalCost = line.Quantity * line.UnitCost;
            totalAmount += totalCost;

            items.Add(new PurchaseOrderItem
            {
                ItemCode = line.ItemCode,
                Quantity = line.Quantity,
                ReceivedQuantity = 0,
                UnitCost = line.UnitCost,
                TotalCost = totalCost
            });
        }

        var order = new PurchaseOrder
        {
            PoNo = poNo,
            VendorId = request.VendorId,
            BranchCode = request.BranchCode,
            PoDate = request.PoDate ?? DateTime.UtcNow,
            ExpectedDate = request.ExpectedDate,
            TotalAmount = totalAmount,
            Remarks = request.Remarks,
            Status = PurchaseOrderStatus.Open,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = items
        };

        await _unitOfWork.PurchaseOrders.AddAsync(order, cancellationToken);

        await _unitOfWork.PurchaseOrderHistories.AddAsync(new PurchaseOrderHistory
        {
            PoNo = poNo,
            Action = PurchaseOrderHistoryAction.Created,
            ChangedBy = createdBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = $"Purchase order created with {items.Count} line item(s), total {totalAmount:N2}."
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purchase order {PoNo} created for vendor {VendorId}, total {TotalAmount:N2}", poNo, request.VendorId, totalAmount);

        return await GetByIdAsync(poNo, cancellationToken);
    }

    public async Task<PurchaseOrderDto> UpdateAsync(string poNo, UpdatePurchaseOrderDto request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(poNo, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", poNo);

        EnsureEditable(order);

        var changes = new List<PurchaseOrderHistoryChange>();

        if (order.ExpectedDate != request.ExpectedDate)
        {
            changes.Add(new PurchaseOrderHistoryChange
            {
                Field = PurchaseOrderChangeField.ExpectedDate,
                OldValue = order.ExpectedDate?.ToString("O"),
                NewValue = request.ExpectedDate?.ToString("O")
            });
            order.ExpectedDate = request.ExpectedDate;
        }

        if (order.Remarks != request.Remarks)
        {
            changes.Add(new PurchaseOrderHistoryChange
            {
                Field = PurchaseOrderChangeField.Remarks,
                OldValue = order.Remarks,
                NewValue = request.Remarks
            });
            order.Remarks = request.Remarks;
        }

        var oldItemCount = order.Items.Count;
        var oldTotal = order.TotalAmount ?? 0;

        foreach (var existingItem in order.Items.ToList())
        {
            _unitOfWork.PurchaseOrderItems.Remove(existingItem);
        }
        order.Items.Clear();

        decimal newTotal = 0;
        foreach (var line in request.Items)
        {
            if (await _unitOfWork.Products.GetByIdAsync(line.ItemCode, cancellationToken) is null)
            {
                throw new BadRequestException($"Product '{line.ItemCode}' does not exist.");
            }

            var totalCost = line.Quantity * line.UnitCost;
            newTotal += totalCost;

            order.Items.Add(new PurchaseOrderItem
            {
                PoNo = poNo,
                ItemCode = line.ItemCode,
                Quantity = line.Quantity,
                ReceivedQuantity = 0,
                UnitCost = line.UnitCost,
                TotalCost = totalCost
            });
        }

        if (oldItemCount != request.Items.Count || oldTotal != newTotal)
        {
            changes.Add(new PurchaseOrderHistoryChange
            {
                Field = PurchaseOrderChangeField.Quantity,
                OldValue = $"{oldItemCount} item(s)",
                NewValue = $"{request.Items.Count} item(s)"
            });
            changes.Add(new PurchaseOrderHistoryChange
            {
                Field = PurchaseOrderChangeField.TotalCost,
                OldValue = oldTotal.ToString("N2"),
                NewValue = newTotal.ToString("N2")
            });
        }

        order.TotalAmount = newTotal;
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.PurchaseOrders.Update(order);

        if (changes.Count > 0)
        {
            var history = new PurchaseOrderHistory
            {
                PoNo = poNo,
                Action = PurchaseOrderHistoryAction.Modified,
                ChangedBy = updatedBy,
                ChangedAt = DateTime.UtcNow,
                Remarks = "Purchase order updated.",
                Changes = changes
            };

            await _unitOfWork.PurchaseOrderHistories.AddAsync(history, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purchase order {PoNo} updated successfully", poNo);

        return await GetByIdAsync(poNo, cancellationToken);
    }

    public async Task<PurchaseOrderDto> ApproveAsync(string poNo, string? remarks, string approvedBy, CancellationToken cancellationToken = default)
    {
        return await RecordReviewDecisionAsync(poNo, PurchaseOrderHistoryAction.Approved, remarks, approvedBy, cancellationToken);
    }

    public async Task<PurchaseOrderDto> RejectAsync(string poNo, string? remarks, string rejectedBy, CancellationToken cancellationToken = default)
    {
        return await RecordReviewDecisionAsync(poNo, PurchaseOrderHistoryAction.Rejected, remarks, rejectedBy, cancellationToken);
    }

    public async Task<PurchaseOrderDto> CancelAsync(string poNo, CancelPurchaseOrderDto request, string cancelledBy, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(poNo, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", poNo);

        EnsureEditable(order);

        var previousStatus = order.Status;
        order.Status = PurchaseOrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.PurchaseOrders.Update(order);

        await _unitOfWork.PurchaseOrderHistories.AddAsync(new PurchaseOrderHistory
        {
            PoNo = poNo,
            Action = PurchaseOrderHistoryAction.Cancelled,
            ChangedBy = cancelledBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = request.Remarks ?? "Purchase order cancelled.",
            Changes = new List<PurchaseOrderHistoryChange>
            {
                new()
                {
                    Field = PurchaseOrderChangeField.Status,
                    OldValue = previousStatus.ToString(),
                    NewValue = order.Status.ToString()
                }
            }
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purchase order {PoNo} cancelled by {CancelledBy}", poNo, cancelledBy);

        return await GetByIdAsync(poNo, cancellationToken);
    }

    public async Task DeleteAsync(string poNo, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(poNo, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", poNo);

        EnsureEditable(order);

        if (await _unitOfWork.PurchaseOrders.HasGrnsAsync(poNo, cancellationToken))
        {
            throw new ConflictException($"Purchase order '{poNo}' has GRNs recorded against it and cannot be deleted.");
        }

        // Items and history (and history changes) cascade-delete via EF configuration.
        _unitOfWork.PurchaseOrders.Remove(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purchase order {PoNo} deleted successfully", poNo);
    }

    private async Task<PurchaseOrderDto> RecordReviewDecisionAsync(
        string poNo,
        PurchaseOrderHistoryAction action,
        string? remarks,
        string changedBy,
        CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.PurchaseOrders.GetByIdAsync(poNo, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", poNo);

        if (order.Status != PurchaseOrderStatus.Open)
        {
            throw new ConflictException($"Purchase order '{poNo}' is {order.Status} and can no longer be reviewed.");
        }

        await _unitOfWork.PurchaseOrderHistories.AddAsync(new PurchaseOrderHistory
        {
            PoNo = poNo,
            Action = action,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = remarks
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purchase order {PoNo} marked {Action} by {ChangedBy}", poNo, action, changedBy);

        return await GetByIdAsync(poNo, cancellationToken);
    }

    private static void EnsureEditable(PurchaseOrder order)
    {
        if (order.Status != PurchaseOrderStatus.Open)
        {
            throw new ConflictException($"Purchase order '{order.PoNo}' is {order.Status} and can no longer be modified.");
        }

        if (order.Items.Any(i => (i.ReceivedQuantity ?? 0) > 0))
        {
            throw new ConflictException($"Purchase order '{order.PoNo}' has already received stock against it and cannot be modified.");
        }
    }
}