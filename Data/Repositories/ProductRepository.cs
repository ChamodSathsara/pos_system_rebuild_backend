using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class ProductRepository : GenericRepository<ProductMaster, string>, IProductRepository
{
    private const string CodePrefix = "ITM";

    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ItemCodeExistsAsync(string itemCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(p => p.ItemCode == itemCode, cancellationToken);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, string? excludeItemCode = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .AnyAsync(p => p.Barcode == barcode && (excludeItemCode == null || p.ItemCode != excludeItemCode), cancellationToken);
    }

    public async Task<bool> HasStockAsync(string itemCode, CancellationToken cancellationToken = default)
    {
        return await Context.Set<StockInventory>().AsNoTracking().AnyAsync(s => s.ItemCode == itemCode, cancellationToken);
    }

    public async Task<string> GenerateNextItemCodeAsync(CancellationToken cancellationToken = default)
    {
        var lastCode = await DbSet
            .AsNoTracking()
            .Where(p => p.ItemCode.StartsWith(CodePrefix))
            .OrderByDescending(p => p.ItemCode)
            .Select(p => p.ItemCode)
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

    public async Task<IReadOnlyList<ProductMaster>> SearchAsync(
        int? categoryId,
        int? brandId,
        bool? isActive,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Tax)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            query = query.Where(p => p.BrandId == brandId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = keyword.Trim();
            query = query.Where(p =>
                p.ItemName.Contains(pattern) ||
                p.ItemCode.Contains(pattern) ||
                (p.Barcode != null && p.Barcode.Contains(pattern)));
        }

        return await query.OrderBy(p => p.ItemName).ToListAsync(cancellationToken);
    }

    public async Task<ProductMaster?> GetByIdWithDetailsAsync(string itemCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Tax)
            .FirstOrDefaultAsync(p => p.ItemCode == itemCode, cancellationToken);
    }
}
