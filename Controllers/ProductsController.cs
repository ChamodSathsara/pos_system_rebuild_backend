using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Product;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>Product master endpoints. ItemCode is the primary key and is immutable once created.</summary>
[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Searches products, optionally filtered by category, brand, active status, or a keyword matched against item name/code/barcode.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] bool? isActive,
        [FromQuery] string? keyword,
        CancellationToken cancellationToken)
    {
        var products = await _productService.SearchAsync(categoryId, brandId, isActive, keyword, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProductDto>>.SuccessResponse(products));
    }

    /// <summary>Retrieves a single product by its item code.</summary>
    [HttpGet("{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string itemCode, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(itemCode, cancellationToken);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
    }

    /// <summary>
    /// Creates a new product. If ItemCode is omitted, one is generated automatically (e.g. ITM00001).
    /// Category, brand, and tax references (if provided) must already exist, and barcodes must be unique.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto request, CancellationToken cancellationToken)
    {
        var product = await _productService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { itemCode = product.ItemCode },
            ApiResponse<ProductDto>.SuccessResponse(product, "Product created successfully."));
    }

    /// <summary>Updates an existing product. ItemCode is immutable.</summary>
    [HttpPut("{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string itemCode, [FromBody] UpdateProductDto request, CancellationToken cancellationToken)
    {
        var product = await _productService.UpdateAsync(itemCode, request, CurrentUserCode, cancellationToken);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(product, "Product updated successfully."));
    }

    /// <summary>Deletes a product. Fails if stock records reference it - deactivate it instead in that case.</summary>
    [HttpDelete("{itemCode}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string itemCode, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(itemCode, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Product deleted successfully."));
    }
}
