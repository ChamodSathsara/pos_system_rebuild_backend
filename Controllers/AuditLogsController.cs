using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.Constants;
using PosApi.DTOs.Security;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = RoleConstants.Admin)]
public class AuditLogsController(IAuditLogService service) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AuditLogPageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? userCode = null,
        [FromQuery] string? action = null,
        [FromQuery] string? tableName = null,
        [FromQuery] string? recordId = null,
        [FromQuery] string? branchCode = null,
        [FromQuery] string? warehouseCode = null,
        [FromQuery] Guid? transactionId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await service.SearchAsync(pageNumber, pageSize, userCode, action, tableName,
            recordId, branchCode, warehouseCode, transactionId, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<AuditLogPageDto>.SuccessResponse(result));
    }

    [HttpGet("{logId:int}")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int logId, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(logId, cancellationToken);
        return Ok(ApiResponse<AuditLogDto>.SuccessResponse(result));
    }
}
