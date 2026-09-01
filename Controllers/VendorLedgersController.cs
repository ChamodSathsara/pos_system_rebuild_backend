using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Vendor;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Vendor ledger endpoints. A ledger is normally created automatically alongside its vendor;
/// this controller exposes it for reporting, manual correction, and recording payments.
/// </summary>
[ApiController]
[Route("api/vendor-ledgers")]
[Authorize]
public class VendorLedgersController : BaseApiController
{
    private readonly IVendorLedgerService _vendorLedgerService;

    public VendorLedgersController(IVendorLedgerService vendorLedgerService)
    {
        _vendorLedgerService = vendorLedgerService;
    }

    /// <summary>Retrieves every vendor ledger.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VendorLedgerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var ledgers = await _vendorLedgerService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<VendorLedgerDto>>.SuccessResponse(ledgers));
    }

    /// <summary>Retrieves a ledger by its own id.</summary>
    [HttpGet("{ledgerId:int}")]
    [ProducesResponseType(typeof(ApiResponse<VendorLedgerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int ledgerId, CancellationToken cancellationToken)
    {
        var ledger = await _vendorLedgerService.GetByIdAsync(ledgerId, cancellationToken);
        return Ok(ApiResponse<VendorLedgerDto>.SuccessResponse(ledger));
    }

    /// <summary>Retrieves the ledger belonging to a specific vendor.</summary>
    [HttpGet("vendor/{vendorId:int}")]
    [ProducesResponseType(typeof(ApiResponse<VendorLedgerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByVendorId(int vendorId, CancellationToken cancellationToken)
    {
        var ledger = await _vendorLedgerService.GetByVendorIdAsync(vendorId, cancellationToken);
        return Ok(ApiResponse<VendorLedgerDto>.SuccessResponse(ledger));
    }

    /// <summary>
    /// Creates a ledger for a vendor that doesn't already have one. Normal vendor creation
    /// provisions this automatically, so this exists mainly for data-migration / self-heal cases.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VendorLedgerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateVendorLedgerDto request, CancellationToken cancellationToken)
    {
        var ledger = await _vendorLedgerService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { ledgerId = ledger.LedgerId },
            ApiResponse<VendorLedgerDto>.SuccessResponse(ledger, "Vendor ledger created successfully."));
    }

    /// <summary>
    /// Administrative correction of a ledger's totals. Recomputes OutstandingBalance as
    /// GrnTotal - ReturnTotal - PaidCredit. Prefer POST .../payments for everyday payments.
    /// </summary>
    [HttpPut("{ledgerId:int}")]
    [ProducesResponseType(typeof(ApiResponse<VendorLedgerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int ledgerId, [FromBody] UpdateVendorLedgerDto request, CancellationToken cancellationToken)
    {
        var ledger = await _vendorLedgerService.UpdateAsync(ledgerId, request, cancellationToken);
        return Ok(ApiResponse<VendorLedgerDto>.SuccessResponse(ledger, "Vendor ledger updated successfully."));
    }

    /// <summary>Records a payment made to the vendor, increasing PaidCredit and reducing the outstanding balance.</summary>
    [HttpPost("vendor/{vendorId:int}/payments")]
    [ProducesResponseType(typeof(ApiResponse<VendorLedgerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordPayment(int vendorId, [FromBody] RecordVendorPaymentDto request, CancellationToken cancellationToken)
    {
        var ledger = await _vendorLedgerService.RecordPaymentAsync(vendorId, request, cancellationToken);
        return Ok(ApiResponse<VendorLedgerDto>.SuccessResponse(ledger, "Payment recorded successfully."));
    }

    /// <summary>
    /// Deletes a ledger. Only orphaned ledgers (not linked to a vendor) can be deleted directly;
    /// delete the vendor itself to remove its ledger.
    /// </summary>
    [HttpDelete("{ledgerId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int ledgerId, CancellationToken cancellationToken)
    {
        await _vendorLedgerService.DeleteAsync(ledgerId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Vendor ledger deleted successfully."));
    }
}
