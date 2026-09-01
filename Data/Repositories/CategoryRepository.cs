using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class CategoryRepository : GenericRepository<Category, int>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasChildCategoriesAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken);
    }

    public async Task<bool> HasProductsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ProductMaster>().AsNoTracking().AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        return await query.OrderBy(c => c.CategoryName).ToListAsync(cancellationToken);
    }
}
