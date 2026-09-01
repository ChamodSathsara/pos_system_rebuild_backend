using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class DiscountRepository : GenericRepository<Discount, string>, IDiscountRepository
{
    private const string CodePrefix = "DIS";

    public DiscountRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> DiscountCodeExistsAsync(string discountCode, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(d => d.DiscountCode == discountCode, cancellationToken);
    }

    public async Task<string> GenerateNextDiscountCodeAsync(CancellationToken cancellationToken = default)
    {
        var lastCode = await DbSet
            .AsNoTracking()
            .Where(d => d.DiscountCode.StartsWith(CodePrefix))
            .OrderByDescending(d => d.DiscountCode)
            .Select(d => d.DiscountCode)
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

    public async Task<Discount?> GetByCodeWithDetailsAsync(string discountCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(d => d.Product)
            .FirstOrDefaultAsync(d => d.DiscountCode == discountCode, cancellationToken);
    }

    public async Task<IReadOnlyList<Discount>> SearchAsync(
        DiscountType? discountType,
        bool? isActive,
        string? itemCode,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Include(d => d.Product).AsQueryable();

        if (discountType.HasValue)
        {
            query = query.Where(d => d.DiscountType == discountType.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(d => d.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(itemCode))
        {
            query = query.Where(d => d.ItemCode == itemCode);
        }

        return await query.OrderBy(d => d.DiscountCode).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Discount>> GetActiveItemDiscountsAsync(string itemCode, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(d => d.IsActive
                && d.ItemCode == itemCode
                && (d.DiscountType == DiscountType.Item || d.DiscountType == DiscountType.Item_Quantity || d.DiscountType == DiscountType.Special)
                && (d.StartDate == null || d.StartDate <= date)
                && (d.EndDate == null || d.EndDate >= date))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Discount>> GetActiveBillDiscountsAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(d => d.IsActive
                && (d.DiscountType == DiscountType.Seasonal || d.DiscountType == DiscountType.Total_Bill)
                && (d.StartDate == null || d.StartDate <= date)
                && (d.EndDate == null || d.EndDate >= date))
            .ToListAsync(cancellationToken);
    }
}
