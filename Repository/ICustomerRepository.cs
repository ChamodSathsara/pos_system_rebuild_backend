using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface ICustomerRepository : IGenericRepository<Customer, string>
{
    Task<bool> CustomerCodeExistsAsync(string customerCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the next sequential customer code (e.g. "CUS00001", "CUS00002", ...) for use when
    /// the caller does not supply one explicitly.
    /// </summary>
    Task<string> GenerateNextCustomerCodeAsync(CancellationToken cancellationToken = default);
}
