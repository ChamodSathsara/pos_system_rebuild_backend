using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Pos;
using PosApi.Exceptions;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

[ApiController]
[Route("api/pos-terminal")]
[Authorize]
public class PosTerminalController : BaseApiController
{
    private readonly IPosTerminalService
        _posTerminalService;

    public PosTerminalController(
        IPosTerminalService posTerminalService)
    {
        _posTerminalService = posTerminalService;
    }

    [HttpGet("items")]
    [ProducesResponseType(
        typeof(
            ApiResponse<
                IReadOnlyList<PosTerminalItemDto>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetItems(
        [FromQuery] string? warehouseCode,
        [FromQuery] int? categoryId,
        [FromQuery] string? keyword,
        [FromQuery] bool onlyAvailable = true,
        CancellationToken cancellationToken = default)
    {
        var branchCode = CurrentBranchCode;

        if (string.IsNullOrWhiteSpace(branchCode))
        {
            throw new BadRequestException(
                "The authenticated user is not " +
                "assigned to a branch.");
        }

        var items =
            await _posTerminalService.GetItemsAsync(
                branchCode,
                warehouseCode,
                categoryId,
                keyword,
                onlyAvailable,
                cancellationToken);

        return Ok(
            ApiResponse<
                IReadOnlyList<PosTerminalItemDto>>
                .SuccessResponse(items));
    }
}