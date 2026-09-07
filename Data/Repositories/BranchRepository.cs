using System.Data;
using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class BranchRepository : GenericRepository<Branch, string>, IBranchRepository
{
    public BranchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> BranchCodeExistsAsync(string branchCode, CancellationToken cancellationToken = default)
    {


        return await DbSet.AsNoTracking().AnyAsync(b => b.BranchCode == branchCode, cancellationToken);
    }

    public async Task<string> GenerateNextBranchCodeAsync(CancellationToken cancellationToken = default)
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
            command.CommandText = "SELECT NEXT VALUE FOR dbo.branch_code_sequence";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return $"BRA{Convert.ToInt32(result):D5}";
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    public async Task<bool> BranchNameExistsAsync(
        string branchName,
        string? excludeBranchCode = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = branchName.Trim();
        return await DbSet.AsNoTracking().AnyAsync(
            b => b.BranchName == normalizedName
                && (excludeBranchCode == null || b.BranchCode != excludeBranchCode),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Branch>> GetByCompanyCodeAsync(string companyCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(b => b.CompanyCode == companyCode)
            .ToListAsync(cancellationToken);
    }
}
