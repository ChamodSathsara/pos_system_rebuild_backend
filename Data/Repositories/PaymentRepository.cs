using Microsoft.EntityFrameworkCore;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;

namespace PosApi.Data.Repositories;

public class PaymentRepository : GenericRepository<Payment, int>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByIdWithDetailsAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Sale)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> SearchAsync(
        string? invoiceNo,
        PaymentMethod? method,
        PaymentStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(invoiceNo))
        {
            query = query.Where(p => p.InvoiceNo == invoiceNo);
        }

        if (method.HasValue)
        {
            query = query.Where(p => p.PaymentMethod == method.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= toDate.Value);
        }

        return await query.OrderByDescending(p => p.PaymentDate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByInvoiceNoAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(p => p.InvoiceNo == invoiceNo)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetCashSalesTotalAsync(
        string branchCode,
        string cashierCode,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(p => p.PaymentMethod == PaymentMethod.Cash
                && p.Status == PaymentStatus.Completed
                && p.PaymentDate >= fromDate
                && p.PaymentDate <= toDate
                && p.Sale != null
                && p.Sale.BranchCode == branchCode
                && p.Sale.CreatedBy == cashierCode)
            .SumAsync(p => p.Amount ?? 0, cancellationToken);
    }
}
