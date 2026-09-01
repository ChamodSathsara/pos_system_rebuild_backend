using AutoMapper;
using PosApi.DTOs.Payment;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PaymentDto>> SearchAsync(
        string? invoiceNo,
        PaymentMethod? method,
        PaymentStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.SearchAsync(invoiceNo, method, status, fromDate, toDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<PaymentDto>>(payments);
    }

    public async Task<PaymentDto> GetByIdAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByIdWithDetailsAsync(paymentId, cancellationToken)
            ?? throw new NotFoundException("Payment", paymentId);

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto request, string receivedBy, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(request.InvoiceNo, cancellationToken)
            ?? throw new NotFoundException("Sale", request.InvoiceNo);

        if (sale.Status is SaleStatus.Cancelled or SaleStatus.Refunded)
        {
            throw new ConflictException($"Sale '{request.InvoiceNo}' is {sale.Status} and cannot accept payments.");
        }

        if (request.Amount <= 0)
        {
            throw new BadRequestException("Payment amount must be greater than zero.");
        }

        var balance = sale.BalanceAmount ?? 0;
        if (request.Amount > balance)
        {
            throw new BadRequestException(
                $"Payment of {request.Amount:N2} exceeds the outstanding balance of {balance:N2} for invoice '{request.InvoiceNo}'.");
        }

        var payment = new Payment
        {
            InvoiceNo = request.InvoiceNo,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
            ReferenceNo = request.ReferenceNo,
            Status = PaymentStatus.Completed,
            ReceivedBy = receivedBy
        };

        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);

        sale.PaidAmount = (sale.PaidAmount ?? 0) + request.Amount;
        sale.BalanceAmount = balance - request.Amount;
        _unitOfWork.Sales.Update(sale);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment {PaymentId} of {Amount:N2} recorded against invoice {InvoiceNo}; new balance {Balance:N2}",
            payment.PaymentId, request.Amount, request.InvoiceNo, sale.BalanceAmount);

        return await GetByIdAsync(payment.PaymentId, cancellationToken);
    }

    public async Task<PaymentDto> CancelAsync(int paymentId, CancelPaymentDto request, string cancelledBy, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByIdWithDetailsAsync(paymentId, cancellationToken)
            ?? throw new NotFoundException("Payment", paymentId);

        if (payment.Status != PaymentStatus.Completed)
        {
            throw new ConflictException($"Payment '{paymentId}' is {payment.Status} and cannot be voided again.");
        }

        if (string.IsNullOrWhiteSpace(payment.InvoiceNo))
        {
            throw new ConflictException($"Payment '{paymentId}' has no invoice recorded and cannot be voided.");
        }

        var sale = await _unitOfWork.Sales.GetByIdAsync(payment.InvoiceNo, cancellationToken)
            ?? throw new NotFoundException("Sale", payment.InvoiceNo);

        sale.PaidAmount = (sale.PaidAmount ?? 0) - (payment.Amount ?? 0);
        if (sale.PaidAmount < 0)
        {
            sale.PaidAmount = 0;
        }
        sale.BalanceAmount = (sale.BalanceAmount ?? 0) + (payment.Amount ?? 0);
        _unitOfWork.Sales.Update(sale);

        payment.Status = PaymentStatus.Cancelled;
        _unitOfWork.Payments.Update(payment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment {PaymentId} voided by {CancelledBy} ({Remarks}); invoice {InvoiceNo} balance restored to {Balance:N2}",
            paymentId, cancelledBy, request.Remarks, payment.InvoiceNo, sale.BalanceAmount);

        return await GetByIdAsync(paymentId, cancellationToken);
    }
}
