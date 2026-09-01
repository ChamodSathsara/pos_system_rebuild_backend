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

    public async Task<IReadOnlyList<Branch>> GetByCompanyCodeAsync(string companyCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(b => b.CompanyCode == companyCode)
            .ToListAsync(cancellationToken);
    }
}
