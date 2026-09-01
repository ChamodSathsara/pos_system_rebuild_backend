using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Organization;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Company management endpoints.
/// </summary>
[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController : BaseApiController
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    /// <summary>
    /// Retrieves every company.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CompanyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var companies = await _companyService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CompanyDto>>.SuccessResponse(companies));
    }

    /// <summary>
    /// Retrieves a single company by code.
    /// </summary>
    [HttpGet("{companyCode}")]
    [ProducesResponseType(typeof(ApiResponse<CompanyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string companyCode, CancellationToken cancellationToken)
    {
        var company = await _companyService.GetByCodeAsync(companyCode, cancellationToken);
        return Ok(ApiResponse<CompanyDto>.SuccessResponse(company));
    }

    /// <summary>
    /// Creates a new company.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CompanyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto request, CancellationToken cancellationToken)
    {
        var company = await _companyService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetByCode),
            new { companyCode = company.CompanyCode },
            ApiResponse<CompanyDto>.SuccessResponse(company, "Company created successfully."));
    }

    /// <summary>
    /// Updates an existing company.
    /// </summary>
    [HttpPut("{companyCode}")]
    [ProducesResponseType(typeof(ApiResponse<CompanyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string companyCode, [FromBody] UpdateCompanyDto request, CancellationToken cancellationToken)
    {
        var company = await _companyService.UpdateAsync(companyCode, request, cancellationToken);
        return Ok(ApiResponse<CompanyDto>.SuccessResponse(company, "Company updated successfully."));
    }

    /// <summary>
    /// Deletes a company. Fails if it still has branches assigned to it.
    /// </summary>
    [HttpDelete("{companyCode}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string companyCode, CancellationToken cancellationToken)
    {
        await _companyService.DeleteAsync(companyCode, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Company deleted successfully."));
    }
}
