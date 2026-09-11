using System.Data;
using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class CustomerRepository : GenericRepository<Customer, string>, ICustomerRepository
{
    private const string CodePrefix = "CUS";

    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.CustomerCode.Contains(term)
                || c.CustomerName.Contains(term)
                || (c.Mobile != null && c.Mobile.Contains(term))
                || (c.Email != null && c.Email.Contains(term)));
        }

        return await query
            .OrderBy(c => c.CustomerName)
            .ThenBy(c => c.CustomerCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CustomerCodeExistsAsync(string customerCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(c => c.CustomerCode == customerCode, cancellationToken);
    }

    public async Task<bool> CustomerNameExistsAsync(string customerName, CancellationToken cancellationToken = default)
    {
        var normalizedName = customerName.Trim().ToUpper();
        return await DbSet.AsNoTracking()
            .AnyAsync(c => c.CustomerName.Trim().ToUpper() == normalizedName, cancellationToken);
    }

    public async Task<string> GenerateNextCustomerCodeAsync(CancellationToken cancellationToken = default)
    {
        // SQL Server sequences are atomic, so simultaneous POS requests can never receive
        // the same customer code. The previous MAX+1 approach had a race condition.
        var connection = Context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        try
        {
            if (shouldClose)
            {
                await connection.OpenAsync(cancellationToken);
            }

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR dbo.customer_code_sequence";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            var nextSequence = Convert.ToInt32(result);
            return $"{CodePrefix}{nextSequence:D5}";
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
