namespace PosApi.Constants;

/// <summary>
/// Well-known role names seeded into the user_role table. Business code should prefer
/// checking specific permissions, but these are handy for coarse-grained [Authorize(Roles = ...)].
/// </summary>
public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
    public const string InventoryClerk = "InventoryClerk";

    /// <summary>Manages a single branch. Report access is restricted to that branch only.</summary>
    public const string BranchManager = "Branch_Manager";
}

/// <summary>
/// Custom claim type names used when issuing JWTs.
/// </summary>
public static class ClaimConstants
{
    public const string UserId = "user_id";
    public const string Role = "role";
    public const string BranchCode = "branch_code";
}

public static class CacheKeys
{
    public const string UserPermissionsPrefix = "user-permissions:";
}

/// <summary>
/// Well-known values for ItemLog.Action, used by the services that raise automatic item_log
/// entries (product edits, price changes, stock changes, damage posting).
/// </summary>
public static class ItemLogActions
{
    public const string ProductUpdated = "ProductUpdated";
    public const string PriceChanged = "PriceChanged";
    public const string StockChanged = "StockChanged";
    public const string Damage = "Damage";
}
