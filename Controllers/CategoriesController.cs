using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Product;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>Product category endpoints. Categories support a self-referencing parent/child tree.</summary>
[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>Retrieves all categories, optionally filtered by active status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(isActive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.SuccessResponse(categories));
    }

    /// <summary>Retrieves a single category by id.</summary>
    [HttpGet("{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(categoryId, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(category));
    }

    /// <summary>Creates a new category. If ParentCategoryId is provided, it must reference an existing category.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto request, CancellationToken cancellationToken)
    {
        var category = await _categoryService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { categoryId = category.CategoryId },
            ApiResponse<CategoryDto>.SuccessResponse(category, "Category created successfully."));
    }

    /// <summary>Updates a category. A category cannot be set as its own parent.</summary>
    [HttpPut("{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int categoryId, [FromBody] UpdateCategoryDto request, CancellationToken cancellationToken)
    {
        var category = await _categoryService.UpdateAsync(categoryId, request, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category updated successfully."));
    }

    /// <summary>Deletes a category. Fails if it has child categories or products assigned to it.</summary>
    [HttpDelete("{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int categoryId, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteAsync(categoryId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Category deleted successfully."));
    }
}
