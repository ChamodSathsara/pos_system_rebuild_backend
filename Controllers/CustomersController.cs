using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.DTOs.Customer;
using PosApi.Exceptions;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Customer management endpoints.
/// </summary>
[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : BaseApiController
{
    private readonly ICustomerService _customerService;
    private readonly IUnitOfWork _unitOfWork;

    public CustomersController(ICustomerService customerService, IUnitOfWork unitOfWork)
    {
        _customerService = customerService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Creates a new customer. Requires an authenticated system_user. If customerCode is
    /// omitted, one is generated automatically (e.g. CUS00001).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.CreateCustomerAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetByCode),
            new { customerCode = customer.CustomerCode },
            ApiResponse<CustomerDto>.SuccessResponse(customer, "Customer created successfully."));
    }

    /// <summary>
    /// Retrieves a single customer by code. Exists primarily so the Location header returned by
    /// POST /api/customers resolves to a real, working resource.
    /// </summary>
    [HttpGet("{customerCode}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string customerCode, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerCode, cancellationToken)
            ?? throw new NotFoundException("Customer", customerCode);

        var dto = new CustomerDto
        {
            CustomerCode = customer.CustomerCode,
            CustomerName = customer.CustomerName,
            Mobile = customer.Mobile,
            Address = customer.Address,
            Email = customer.Email,
            CustomerType = customer.CustomerType,
            LoyaltyPoints = customer.LoyaltyPoints,
            CreditLimit = customer.CreditLimit,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };

        return Ok(ApiResponse<CustomerDto>.SuccessResponse(dto));
    }
}
