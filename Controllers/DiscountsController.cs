using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Discount;
using PosApi.Models.Enums;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Discount rule endpoints. Supports five discount types (Item, Item_Quantity, Seasonal,
/// Total_Bill, Special) applied either as a Percentage or a Fixed_Amount. DiscountCode is the
/// primary key. Use the evaluate endpoint at checkout time to resolve which discounts currently
/// apply to a given item/quantity/bill-amount and how much they're worth.
/// </summary>
[ApiController]
[Route("api/discounts")]
[Authorize]
public class DiscountsController : BaseApiController
{
    private readonly IDiscountService _discountService;

    public DiscountsController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    /// <summary>Retrieves all discounts, optionally filtered by type, active status, and/or item.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DiscountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DiscountType? discountType,
        [FromQuery] bool? isActive,
        [FromQuery] string? itemCode,
        CancellationToken cancellationToken)
    {
        var discounts = await _discountService.GetAllAsync(discountType, isActive, itemCode, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DiscountDto>>.SuccessResponse(discounts));
    }

    /// <summary>Retrieves a single discount by code.</summary>
    [HttpGet("{discountCode}")]
    [ProducesResponseType(typeof(ApiResponse<DiscountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string discountCode, CancellationToken cancellationToken)
    {
        var discount = await _discountService.GetByCodeAsync(discountCode, cancellationToken);
        return Ok(ApiResponse<DiscountDto>.SuccessResponse(discount));
    }

    /// <summary>
    /// Resolves which discounts currently apply for a given item/quantity/bill-amount at checkout
    /// time, and calculates how much each is worth. Supply ItemCode (+ optionally Quantity and
    /// ItemAmount) to evaluate item-level discounts, and/or BillAmount to evaluate bill-level ones.
    /// </summary>
    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(ApiResponse<DiscountEvaluationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Evaluate([FromBody] EvaluateDiscountRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _discountService.EvaluateAsync(request, cancellationToken);
        return Ok(ApiResponse<DiscountEvaluationResultDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Creates a new discount. Required fields vary by DiscountType: Item/Item_Quantity/Special
    /// require ItemCode; Item_Quantity also requires MinQuantity; Seasonal/Special require
    /// StartDate+EndDate; Total_Bill requires MinBillAmount. ApplicableTo is derived automatically.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DiscountDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountDto request, CancellationToken cancellationToken)
    {
        var discount = await _discountService.CreateAsync(request, CurrentUserCode, cancellationToken);

        return CreatedAtAction(
            nameof(GetByCode),
            new { discountCode = discount.DiscountCode },
            ApiResponse<DiscountDto>.SuccessResponse(discount, "Discount created successfully."));
    }

    /// <summary>Updates a discount's rule fields and active status. DiscountCode and DiscountType are immutable once created.</summary>
    [HttpPut("{discountCode}")]
    [ProducesResponseType(typeof(ApiResponse<DiscountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string discountCode, [FromBody] UpdateDiscountDto request, CancellationToken cancellationToken)
    {
        var discount = await _discountService.UpdateAsync(discountCode, request, cancellationToken);
        return Ok(ApiResponse<DiscountDto>.SuccessResponse(discount, "Discount updated successfully."));
    }

    /// <summary>Deletes a discount.</summary>
    [HttpDelete("{discountCode}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string discountCode, CancellationToken cancellationToken)
    {
        await _discountService.DeleteAsync(discountCode, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Discount deleted successfully."));
    }
}
