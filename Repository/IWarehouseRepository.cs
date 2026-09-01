using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IWarehouseRepository : IGenericRepository<Warehouse, string>
{
    Task<bool> WarehouseCodeExistsAsync(string warehouseCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Warehouse>> GetByBranchCodeAsync(string branchCode, CancellationToken cancellationToken = default);
}
