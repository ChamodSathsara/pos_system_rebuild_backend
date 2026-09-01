using AutoMapper;
using PosApi.DTOs.Purchase;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class PurchaseOrderHistoryService : IPurchaseOrderHistoryService
{
    private static readonly PurchaseOrderHistoryAction[] ImmutableSystemActions =
    {
        PurchaseOrderHistoryAction.Created,
        PurchaseOrderHistoryAction.Modified,
        PurchaseOrderHistoryAction.Cancelled,
        PurchaseOrderHistoryAction.StatusChanged
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PurchaseOrderHistoryService> _logger;

    public PurchaseOrderHistoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseOrderHistoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PurchaseOrderHistoryDto>> GetByPoNoAsync(string poNo, CancellationToken cancellationToken = default)
    {
        var histories = await _unitOfWork.PurchaseOrderHistories.GetByPoNoAsync(poNo, cancellationToken);
        return _mapper.Map<IReadOnlyList<PurchaseOrderHistoryDto>>(histories);
    }

    public async Task<PurchaseOrderHistoryDto> GetByIdAsync(int historyId, CancellationToken cancellationToken = default)
    {
        var history = await _unitOfWork.PurchaseOrderHistories.GetByIdWithChangesAsync(historyId, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrderHistory", historyId);

        return _mapper.Map<PurchaseOrderHistoryDto>(history);
    }

    public async Task<PurchaseOrderHistoryDto> CreateAsync(CreatePurchaseOrderHistoryDto request, string changedBy, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.PurchaseOrders.GetByIdAsync(request.PoNo, cancellationToken) is null)
        {
            throw new NotFoundException("PurchaseOrder", request.PoNo);
        }

        var history = new PurchaseOrderHistory
        {
            PoNo = request.PoNo,
            Action = request.Action,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            Remarks = request.Remarks
        };

        await _unitOfWork.PurchaseOrderHistories.AddAsync(history, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("History entry {Action} recorded for purchase order {PoNo}", request.Action, request.PoNo);

        return _mapper.Map<PurchaseOrderHistoryDto>(history);
    }

    public async Task<PurchaseOrderHistoryDto> UpdateAsync(int historyId, UpdatePurchaseOrderHistoryDto request, CancellationToken cancellationToken = default)
    {
        var history = await _unitOfWork.PurchaseOrderHistories.GetByIdAsync(historyId, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrderHistory", historyId);

        // Action, ChangedBy, and ChangedAt are immutable audit facts - only the note is editable.
        history.Remarks = request.Remarks;

        _unitOfWork.PurchaseOrderHistories.Update(history);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(historyId, cancellationToken);
    }

    public async Task DeleteAsync(int historyId, CancellationToken cancellationToken = default)
    {
        var history = await _unitOfWork.PurchaseOrderHistories.GetByIdAsync(historyId, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrderHistory", historyId);

        if (ImmutableSystemActions.Contains(history.Action))
        {
            throw new ConflictException(
                $"History entries of type '{history.Action}' are generated automatically by the purchase order lifecycle and cannot be deleted.");
        }

        _unitOfWork.PurchaseOrderHistories.Remove(history);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("History entry {HistoryId} deleted successfully", historyId);
    }
}