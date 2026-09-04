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

    public async Task<bool> CustomerCodeExistsAsync(string customerCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(c => c.CustomerCode == customerCode, cancellationToken);
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
