using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Stock;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

[ApiController]
[Route("api/opening-stocks")]
[Authorize]
public class OpeningStocksController : BaseApiController
{
    private readonly IOpeningStockService _openingStockService;

    public OpeningStocksController(
        IOpeningStockService openingStockService)
    {
        _openingStockService = openingStockService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<OpeningStockDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOpeningStockDto request,
        CancellationToken cancellationToken)
    {
        var result = await _openingStockService.CreateAsync(
            request,
            CurrentUserCode,
            CurrentBranchCode,
            CurrentRole,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<OpeningStockDto>.SuccessResponse(
                result,
                "Opening stock applied successfully."));
    }
}