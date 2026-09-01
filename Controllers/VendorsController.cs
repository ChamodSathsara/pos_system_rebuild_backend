using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Vendor;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Vendor (supplier) management endpoints. Creating a vendor automatically provisions a
/// zeroed vendor ledger for it - see VendorLedgersController for ledger operations.
/// </summary>
[ApiController]
[Route("api/vendors")]
[Authorize]
public class VendorsController : BaseApiController
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    /// <summary>Retrieves all vendors, optionally filtered by active status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VendorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var vendors = await _vendorService.GetAllAsync(isActive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<VendorDto>>.SuccessResponse(vendors));
    }

    /// <summary>Retrieves a single vendor by its numeric id, including its outstanding balance.</summary>
    [HttpGet("{vendorId:int}")]
    [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int vendorId, CancellationToken cancellationToken)
    {
        var vendor = await _vendorService.GetByIdAsync(vendorId, cancellationToken);
        return Ok(ApiResponse<VendorDto>.SuccessResponse(vendor));
    }

    /// <summary>Retrieves a single vendor by its vendor code.</summary>
    [HttpGet("code/{vendorCode}")]
    [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string vendorCode, CancellationToken cancellationToken)
    {
        var vendor = await _vendorService.GetByCodeAsync(vendorCode, cancellationToken);
        return Ok(ApiResponse<VendorDto>.SuccessResponse(vendor));
    }

    /// <summary>
    /// Creates a new vendor. If vendorCode is omitted, one is generated automatically
    /// (e.g. VEN00001). A zero-balance ledger is created for the vendor automatically.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateVendorDto request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { vendorId = vendor.VendorId },
            ApiResponse<VendorDto>.SuccessResponse(vendor, "Vendor created successfully."));
    }

    /// <summary>Updates an existing vendor. VendorCode is immutable.</summary>
    [HttpPut("{vendorId:int}")]
    [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int vendorId, [FromBody] UpdateVendorDto request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorService.UpdateAsync(vendorId, request, cancellationToken);
        return Ok(ApiResponse<VendorDto>.SuccessResponse(vendor, "Vendor updated successfully."));
    }

    /// <summary>
    /// Deletes a vendor. Fails if the vendor has purchase orders or GRNs referencing it, or if
    /// its ledger still carries an outstanding balance.
    /// </summary>
    [HttpDelete("{vendorId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int vendorId, CancellationToken cancellationToken)
    {
        await _vendorService.DeleteAsync(vendorId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Vendor deleted successfully."));
    }
}
