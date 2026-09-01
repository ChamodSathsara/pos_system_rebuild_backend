using System.Security.Claims;
using PosApi.Constants;

namespace PosApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Reads the authenticated user's user_code (system_user primary key) from the "user_id" claim.
    /// </summary>
    public static string? GetUserCode(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimConstants.UserId)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string? GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimConstants.Role)
            ?? principal.FindFirstValue(ClaimTypes.Role);
    }

    public static string? GetUsername(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name);
    }

    /// <summary>
    /// Reads the authenticated user's assigned branch_code from the JWT, if any. Present for
    /// users tied to a single branch (e.g. a Branch_Manager or Cashier); null for head-office
    /// roles such as Admin/Manager that are not restricted to one branch.
    /// </summary>
    public static string? GetBranchCode(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimConstants.BranchCode);
    }
}
