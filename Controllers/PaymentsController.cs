using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Payment;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Payment endpoints. Payments are recorded against a sale's outstanding balance and can be
/// voided individually; each create/cancel keeps the parent sale's PaidAmount/BalanceAmount in
/// sync.
/// </summary>
[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Searches payments, optionally filtered by invoice, method, status, or payment date range.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? invoiceNo,
        [FromQuery] PaymentMethod? method,
        [FromQuery] PaymentStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentService.SearchAsync(invoiceNo, method, status, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PaymentDto>>.SuccessResponse(payments));
    }

    /// <summary>Retrieves a single payment.</summary>
    [HttpGet("{paymentId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int paymentId, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetByIdAsync(paymentId, cancellationToken);
        return Ok(ApiResponse<PaymentDto>.SuccessResponse(payment));
    }

    /// <summary>Records a payment against a sale. The amount can never exceed the sale's current outstanding balance.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto request, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { paymentId = payment.PaymentId },
            ApiResponse<PaymentDto>.SuccessResponse(payment, "Payment recorded successfully."));
    }

    /// <summary>Voids a previously recorded payment, restoring the amount to the sale's outstanding balance.</summary>
    [HttpPost("{paymentId:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int paymentId, [FromBody] CancelPaymentDto request, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.CancelAsync(paymentId, request, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<PaymentDto>.SuccessResponse(payment, "Payment voided successfully."));
    }
}
