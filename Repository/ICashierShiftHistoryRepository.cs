using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface ICashierShiftHistoryRepository : IGenericRepository<CashierShiftHistory, int>
{
    Task<IReadOnlyList<CashierShiftHistory>> GetByShiftIdAsync(int shiftId, CancellationToken cancellationToken = default);
}
