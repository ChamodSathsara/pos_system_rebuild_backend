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
        var lastCode = await DbSet
            .AsNoTracking()
            .Where(c => c.CustomerCode.StartsWith(CodePrefix))
            .OrderByDescending(c => c.CustomerCode)
            .Select(c => c.CustomerCode)
            .FirstOrDefaultAsync(cancellationToken);

        var nextSequence = 1;
        if (lastCode is not null && lastCode.Length > CodePrefix.Length)
        {
            var numericPart = lastCode[CodePrefix.Length..];
            if (int.TryParse(numericPart, out var parsed))
            {
                nextSequence = parsed + 1;
            }
        }

        return $"{CodePrefix}{nextSequence:D5}";
    }
}
