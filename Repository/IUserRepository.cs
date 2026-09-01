using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IUserRepository : IGenericRepository<SystemUser, string>
{
    /// <summary>
    /// Loads a system_user together with its role (needed to build JWT role claims), by username.
    /// </summary>
    Task<SystemUser?> GetByUsernameWithRoleAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a system_user together with its role, by user_code (primary key).
    /// </summary>
    Task<SystemUser?> GetByUserCodeWithRoleAsync(string userCode, CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads every system_user together with its role (needed so list views can show RoleName).
    /// </summary>
    Task<IReadOnlyList<SystemUser>> GetAllWithRoleAsync(CancellationToken cancellationToken = default);
}
