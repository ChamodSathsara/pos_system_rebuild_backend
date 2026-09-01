using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class CompanyRepository : GenericRepository<Company, string>, ICompanyRepository
{
    public CompanyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> CompanyCodeExistsAsync(string companyCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(c => c.CompanyCode == companyCode, cancellationToken);
    }
}
