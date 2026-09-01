using AutoMapper;
using PosApi.DTOs.Customer;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto request, CancellationToken cancellationToken = default)
    {
        var customerCode = request.CustomerCode?.Trim();

        if (string.IsNullOrWhiteSpace(customerCode))
        {
            customerCode = await _unitOfWork.Customers.GenerateNextCustomerCodeAsync(cancellationToken);
        }
        else if (await _unitOfWork.Customers.CustomerCodeExistsAsync(customerCode, cancellationToken))
        {
            throw new ConflictException($"A customer with code '{customerCode}' already exists.");
        }

        var customer = new Customer
        {
            CustomerCode = customerCode,
            CustomerName = request.CustomerName.Trim(),
            Mobile = request.Mobile,
            Address = request.Address,
            Email = request.Email,
            CustomerType = request.CustomerType,
            CreditLimit = request.CreditLimit,
            LoyaltyPoints = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {CustomerCode} created successfully", customer.CustomerCode);

        return _mapper.Map<CustomerDto>(customer);
    }
}
