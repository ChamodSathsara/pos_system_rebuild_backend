using PosApi.Models.Entities;
using PosApi.Models.Enums;

namespace PosApi.Repository;

public interface ICashierShiftRepository : IGenericRepository<CashierShift, int>
{
    Task<CashierShift?> GetByIdWithDetailsAsync(int shiftId, CancellationToken cancellationToken = default);

    /// <summary>Returns the cashier's currently Open shift at the given branch, if any.</summary>
    Task<CashierShift?> GetOpenShiftAsync(string cashierCode, string branchCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CashierShift>> SearchAsync(
        string? branchCode,
        string? cashierCode,
        CashierShiftStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);
}
