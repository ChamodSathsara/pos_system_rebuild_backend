using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Product;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>Product brand endpoints.</summary>
[ApiController]
[Route("api/brands")]
[Authorize]
public class BrandsController : BaseApiController
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    /// <summary>Retrieves all brands, optionally filtered by active status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BrandDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var brands = await _brandService.GetAllAsync(isActive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BrandDto>>.SuccessResponse(brands));
    }

    /// <summary>Retrieves a single brand by id.</summary>
    [HttpGet("{brandId:int}")]
    [ProducesResponseType(typeof(ApiResponse<BrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int brandId, CancellationToken cancellationToken)
    {
        var brand = await _brandService.GetByIdAsync(brandId, cancellationToken);
        return Ok(ApiResponse<BrandDto>.SuccessResponse(brand));
    }

    /// <summary>Creates a new brand.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BrandDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBrandDto request, CancellationToken cancellationToken)
    {
        var brand = await _brandService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { brandId = brand.BrandId },
            ApiResponse<BrandDto>.SuccessResponse(brand, "Brand created successfully."));
    }

    /// <summary>Updates a brand.</summary>
    [HttpPut("{brandId:int}")]
    [ProducesResponseType(typeof(ApiResponse<BrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int brandId, [FromBody] UpdateBrandDto request, CancellationToken cancellationToken)
    {
        var brand = await _brandService.UpdateAsync(brandId, request, cancellationToken);
        return Ok(ApiResponse<BrandDto>.SuccessResponse(brand, "Brand updated successfully."));
    }

    /// <summary>Deletes a brand. Fails if any products are assigned to it.</summary>
    [HttpDelete("{brandId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int brandId, CancellationToken cancellationToken)
    {
        await _brandService.DeleteAsync(brandId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Brand deleted successfully."));
    }
}
