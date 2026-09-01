using PosApi.DTOs.Customer;

namespace PosApi.Service.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto request, CancellationToken cancellationToken = default);
}
