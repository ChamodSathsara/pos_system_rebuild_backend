using PosApi.DTOs.Stock;
using PosApi.Models.Enums;

namespace PosApi.Service.Interfaces;

public interface IDamageItemService
{
    Task<IReadOnlyList<DamageItemDto>> SearchAsync(
        string? itemCode,
        string? branchCode,
        string? warehouseCode,
        DamageItemStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<DamageItemDto> GetByIdAsync(int damageId, CancellationToken cancellationToken = default);

    /// <summary>Records a new damage report. ReportedBy is always set to the currently authenticated user.</summary>
    Task<DamageItemDto> CreateAsync(CreateDamageItemDto request, string reportedBy, CancellationToken cancellationToken = default);

    Task<DamageItemDto> UpdateAsync(int damageId, UpdateDamageItemDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int damageId, CancellationToken cancellationToken = default);
}
