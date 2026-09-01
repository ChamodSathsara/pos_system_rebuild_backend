using AutoMapper;
using PosApi.DTOs.Product;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class ItemLogService : IItemLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ItemLogService> _logger;

    public ItemLogService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ItemLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ItemLogDto>> SearchAsync(
        string? itemCode,
        string? action,
        string? changedBy,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var itemLogs = await _unitOfWork.ItemLogs.SearchAsync(itemCode, action, changedBy, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<ItemLogDto>>(itemLogs);
    }

    public async Task<ItemLogDto> GetByIdAsync(int logId, CancellationToken cancellationToken = default)
    {
        var itemLog = await _unitOfWork.ItemLogs.GetByIdWithDetailsAsync(logId, cancellationToken)
            ?? throw new NotFoundException("ItemLog", logId);

        return _mapper.Map<ItemLogDto>(itemLog);
    }

    public async Task<ItemLogDto> CreateAsync(CreateItemLogDto request, string changedBy, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ItemCode, cancellationToken)
            ?? throw new NotFoundException("Product", request.ItemCode);

        var itemLog = new ItemLog
        {
            ItemCode = product.ItemCode,
            Action = request.Action,
            OldValue = request.OldValue,
            NewValue = request.NewValue,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        };

        await _unitOfWork.ItemLogs.AddAsync(itemLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Item log {LogId} recorded for item {ItemCode} ({Action}) by {ChangedBy}",
            itemLog.LogId, product.ItemCode, request.Action, changedBy);

        return await GetByIdAsync(itemLog.LogId, cancellationToken);
    }

    public async Task DeleteAsync(int logId, CancellationToken cancellationToken = default)
    {
        var itemLog = await _unitOfWork.ItemLogs.GetByIdAsync(logId, cancellationToken)
            ?? throw new NotFoundException("ItemLog", logId);

        _unitOfWork.ItemLogs.Remove(itemLog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Item log {LogId} deleted successfully", logId);
    }
}
