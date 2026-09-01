using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Product;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>Tax master endpoints. TaxCode is the primary key and is immutable once created.</summary>
[ApiController]
[Route("api/tax-masters")]
[Authorize]
public class TaxMastersController : BaseApiController
{
    private readonly ITaxMasterService _taxMasterService;

    public TaxMastersController(ITaxMasterService taxMasterService)
    {
        _taxMasterService = taxMasterService;
    }

    /// <summary>Retrieves all tax codes, optionally filtered by active status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TaxMasterDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var taxes = await _taxMasterService.GetAllAsync(isActive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TaxMasterDto>>.SuccessResponse(taxes));
    }

    /// <summary>Retrieves a single tax code.</summary>
    [HttpGet("{taxCode}")]
    [ProducesResponseType(typeof(ApiResponse<TaxMasterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string taxCode, CancellationToken cancellationToken)
    {
        var tax = await _taxMasterService.GetByCodeAsync(taxCode, cancellationToken);
        return Ok(ApiResponse<TaxMasterDto>.SuccessResponse(tax));
    }

    /// <summary>Creates a new tax code.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaxMasterDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTaxMasterDto request, CancellationToken cancellationToken)
    {
        var tax = await _taxMasterService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetByCode),
            new { taxCode = tax.TaxCode },
            ApiResponse<TaxMasterDto>.SuccessResponse(tax, "Tax created successfully."));
    }

    /// <summary>Updates a tax code's name, percentage, description, and active status. TaxCode itself is immutable.</summary>
    [HttpPut("{taxCode}")]
    [ProducesResponseType(typeof(ApiResponse<TaxMasterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string taxCode, [FromBody] UpdateTaxMasterDto request, CancellationToken cancellationToken)
    {
        var tax = await _taxMasterService.UpdateAsync(taxCode, request, cancellationToken);
        return Ok(ApiResponse<TaxMasterDto>.SuccessResponse(tax, "Tax updated successfully."));
    }

    /// <summary>Deletes a tax code. Fails if any products reference it.</summary>
    [HttpDelete("{taxCode}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string taxCode, CancellationToken cancellationToken)
    {
        await _taxMasterService.DeleteAsync(taxCode, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Tax deleted successfully."));
    }
}
