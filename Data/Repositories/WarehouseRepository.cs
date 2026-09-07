using System.Data;
using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class WarehouseRepository : GenericRepository<Warehouse, string>, IWarehouseRepository
{
    public WarehouseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> WarehouseCodeExistsAsync(string warehouseCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(w => w.WarehouseCode == warehouseCode, cancellationToken);
    }

    public async Task<string> GenerateNextWarehouseCodeAsync(CancellationToken cancellationToken = default)
    {
        var connection = Context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        try
        {
            if (shouldClose)
            {
                await connection.OpenAsync(cancellationToken);
            }

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR dbo.warehouse_code_sequence";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return $"WH{Convert.ToInt32(result):D3}";
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    public async Task<IReadOnlyList<Warehouse>> GetByBranchCodeAsync(string branchCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(w => w.BranchCode == branchCode)
            .ToListAsync(cancellationToken);
    }
}
