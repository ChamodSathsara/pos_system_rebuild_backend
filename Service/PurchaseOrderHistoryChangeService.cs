using AutoMapper;
using PosApi.DTOs.Purchase;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class PurchaseOrderHistoryChangeService : IPurchaseOrderHistoryChangeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PurchaseOrderHistoryChangeService> _logger;

    public PurchaseOrderHistoryChangeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseOrderHistoryChangeService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PurchaseOrderHistoryChangeDto>> GetByHistoryIdAsync(int historyId, CancellationToken cancellationToken = default)
    {
        var changes = await _unitOfWork.PurchaseOrderHistoryChanges.GetByHistoryIdAsync(historyId, cancellationToken);
        return _mapper.Map<IReadOnlyList<PurchaseOrderHistoryChangeDto>>(changes);
    }

    public async Task<PurchaseOrderHistoryChangeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var change = await _unitOfWork.PurchaseOrderHistoryChanges.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrderHistoryChange", id);

        return _mapper.Map<PurchaseOrderHistoryChangeDto>(change);
    }

    public async Task<PurchaseOrderHistoryChangeDto> CreateAsync(CreatePurchaseOrderHistoryChangeDto request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.PurchaseOrderHistories.GetByIdAsync(request.HistoryId, cancellationToken) is null)
        {
            throw new NotFoundException("PurchaseOrderHistory", request.HistoryId);
        }

        var change = new PurchaseOrderHistoryChange
        {
            HistoryId = request.HistoryId,
            Field = request.Field,
            OldValue = request.OldValue,
            NewValue = request.NewValue
        };

        await _unitOfWork.PurchaseOrderHistoryChanges.AddAsync(change, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("History change entry created for history {HistoryId}", request.HistoryId);

        return _mapper.Map<PurchaseOrderHistoryChangeDto>(change);
    }

    public async Task<PurchaseOrderHistoryChangeDto> UpdateAsync(int id, UpdatePurchaseOrderHistoryChangeDto request, CancellationToken cancellationToken = default)
    {
        var change = await _unitOfWork.PurchaseOrderHistoryChanges.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrderHistoryChange", id);

        change.Field = request.Field;
        change.OldValue = request.OldValue;
        change.NewValue = request.NewValue;

        _unitOfWork.PurchaseOrderHistoryChanges.Update(change);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PurchaseOrderHistoryChangeDto>(change);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var change = await _unitOfWork.PurchaseOrderHistoryChanges.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrderHistoryChange", id);

        _unitOfWork.PurchaseOrderHistoryChanges.Remove(change);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("History change entry {Id} deleted successfully", id);
    }
}