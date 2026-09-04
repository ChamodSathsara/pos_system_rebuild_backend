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

    public async Task<string> GenerateNextItemCodeAsync(
    CancellationToken cancellationToken = default)
    {
        var itemCodes = await DbSet
            .AsNoTracking()
            .Where(product =>
                product.ItemCode.StartsWith(CodePrefix))
            .Select(product => product.ItemCode)
            .ToListAsync(cancellationToken);

        var maximumSequence = itemCodes
            .Select(itemCode =>
            {
                var numericPart =
                    itemCode[CodePrefix.Length..];

                return int.TryParse(
                    numericPart,
                    out var parsed)
                        ? parsed
                        : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        var nextSequence = maximumSequence + 1;

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

    public async Task<IReadOnlyList<ProductMaster>> GetByIdsWithDetailsAsync(
        IReadOnlyCollection<string> itemCodes,
        CancellationToken cancellationToken = default)
    {
        if (itemCodes.Count == 0)
        {
            return Array.Empty<ProductMaster>();
        }

        return await DbSet
            .AsNoTracking()
            .Include(p => p.Tax)
            .Where(p => itemCodes.Contains(p.ItemCode))
            .ToListAsync(cancellationToken);
    }
}
