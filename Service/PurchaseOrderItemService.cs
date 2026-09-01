using AutoMapper;
using PosApi.DTOs.Purchase;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class PurchaseOrderItemService : IPurchaseOrderItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PurchaseOrderItemService> _logger;

    public PurchaseOrderItemService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseOrderItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PurchaseOrderItemDto>> GetByPoNoAsync(string poNo, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.PurchaseOrderItems.GetByPoNoAsync(poNo, cancellationToken);
        return _mapper.Map<IReadOnlyList<PurchaseOrderItemDto>>(items);
    }

    public async Task<PurchaseOrderItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.PurchaseOrderItems.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrderItem", id);

        return _mapper.Map<PurchaseOrderItemDto>(item);
    }

    public async Task<PurchaseOrderItemDto> CreateAsync(CreatePurchaseOrderItemDto request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(request.PoNo, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", request.PoNo);

        EnsureEditable(order);

        if (await _unitOfWork.Products.GetByIdAsync(request.ItemCode, cancellationToken) is null)
        {
            throw new BadRequestException($"Product '{request.ItemCode}' does not exist.");
        }

        if (order.Items.Any(i => i.ItemCode == request.ItemCode))
        {
            throw new ConflictException($"Purchase order '{request.PoNo}' already has a line item for '{request.ItemCode}'. Update the existing line instead.");
        }

        var totalCost = request.Quantity * request.UnitCost;

        var item = new PurchaseOrderItem
        {
            PoNo = request.PoNo,
            ItemCode = request.ItemCode,
            Quantity = request.Quantity,
            ReceivedQuantity = 0,
            UnitCost = request.UnitCost,
            TotalCost = totalCost
        };

        await _unitOfWork.PurchaseOrderItems.AddAsync(item, cancellationToken);

        order.TotalAmount = (order.TotalAmount ?? 0) + totalCost;
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.PurchaseOrders.Update(order);

        await _unitOfWork.PurchaseOrderHistories.AddAsync(new PurchaseOrderHistory
        {
            PoNo = request.PoNo,
            Action = PurchaseOrderHistoryAction.Modified,
            ChangedBy = updatedBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = $"Line item added: {request.ItemCode} x {request.Quantity} @ {request.UnitCost:N2}."
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Item {ItemCode} added to purchase order {PoNo}", request.ItemCode, request.PoNo);

        return _mapper.Map<PurchaseOrderItemDto>(item);
    }

    public async Task<PurchaseOrderItemDto> UpdateAsync(int id, UpdatePurchaseOrderItemDto request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.PurchaseOrderItems.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrderItem", id);

        var order = await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(item.PoNo!, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", item.PoNo ?? string.Empty);

        EnsureEditable(order);

        if ((item.ReceivedQuantity ?? 0) > 0)
        {
            throw new ConflictException($"Line item {id} has already received stock and cannot be modified.");
        }

        var oldQuantity = item.Quantity;
        var oldUnitCost = item.UnitCost;
        var oldTotalCost = item.TotalCost ?? 0;
        var newTotalCost = request.Quantity * request.UnitCost;

        item.Quantity = request.Quantity;
        item.UnitCost = request.UnitCost;
        item.TotalCost = newTotalCost;
        _unitOfWork.PurchaseOrderItems.Update(item);

        order.TotalAmount = (order.TotalAmount ?? 0) - oldTotalCost + newTotalCost;
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.PurchaseOrders.Update(order);

        await _unitOfWork.PurchaseOrderHistories.AddAsync(new PurchaseOrderHistory
        {
            PoNo = item.PoNo,
            Action = PurchaseOrderHistoryAction.Modified,
            ChangedBy = updatedBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = $"Line item {item.ItemCode} updated.",
            Changes = new List<PurchaseOrderHistoryChange>
            {
                new() { Field = PurchaseOrderChangeField.Quantity, OldValue = oldQuantity?.ToString("N3"), NewValue = request.Quantity.ToString("N3") },
                new() { Field = PurchaseOrderChangeField.UnitCost, OldValue = oldUnitCost?.ToString("N2"), NewValue = request.UnitCost.ToString("N2") }
            }
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purchase order item {Id} updated successfully", id);

        return _mapper.Map<PurchaseOrderItemDto>(item);
    }

    public async Task DeleteAsync(int id, string updatedBy, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.PurchaseOrderItems.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrderItem", id);

        var order = await _unitOfWork.PurchaseOrders.GetByIdWithItemsAsync(item.PoNo!, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", item.PoNo ?? string.Empty);

        EnsureEditable(order);

        if ((item.ReceivedQuantity ?? 0) > 0)
        {
            throw new ConflictException($"Line item {id} has already received stock and cannot be deleted.");
        }

        if (order.Items.Count <= 1)
        {
            throw new ConflictException("Cannot delete the only line item on a purchase order; delete the purchase order instead.");
        }

        _unitOfWork.PurchaseOrderItems.Remove(item);

        order.TotalAmount = (order.TotalAmount ?? 0) - (item.TotalCost ?? 0);
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.PurchaseOrders.Update(order);

        await _unitOfWork.PurchaseOrderHistories.AddAsync(new PurchaseOrderHistory
        {
            PoNo = item.PoNo,
            Action = PurchaseOrderHistoryAction.Modified,
            ChangedBy = updatedBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = $"Line item {item.ItemCode} removed."
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Purchase order item {Id} deleted successfully", id);
    }

    private static void EnsureEditable(PurchaseOrder order)
    {
        if (order.Status != PurchaseOrderStatus.Open)
        {
            throw new ConflictException($"Purchase order '{order.PoNo}' is {order.Status} and can no longer be modified.");
        }
    }
}