using Microsoft.AspNetCore.Mvc;
using PosApi.Extensions;

namespace PosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// The user_code of the currently authenticated caller, taken from the "user_id" JWT claim.
    /// Throws if called from an unauthenticated context - only use inside [Authorize] actions.
    /// </summary>
    protected string CurrentUserCode =>
        User.GetUserCode() ?? throw new InvalidOperationException("No authenticated user found on the current request.");

    /// <summary>The caller's role name (e.g. "Admin", "Manager", "Branch_Manager", "Cashier"), if present on the token.</summary>
    protected string? CurrentRole => User.GetRole();

    /// <summary>The caller's assigned branch_code, if present on the token. Null for head-office roles.</summary>
    protected string? CurrentBranchCode => User.GetBranchCode();
}
