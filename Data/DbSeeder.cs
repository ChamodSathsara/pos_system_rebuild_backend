using Microsoft.EntityFrameworkCore;
using PosApi.Constants;
using PosApi.Models.Entities;
using PosApi.Security;

namespace PosApi.Data;

/// <summary>
/// Idempotent startup seeding: ensures the database is migrated and a baseline set of
/// user_role rows plus a default admin system_user exist so the API is usable immediately
/// after first run. Safe to call on every startup.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(context);
        await SeedDefaultAdminAsync(context, passwordHasher);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        var existingRoleNames = await context.UserRoles.Select(r => r.RoleName).ToListAsync();

        var defaultRoles = new[]
        {
            RoleConstants.Admin,
            RoleConstants.Manager,
            RoleConstants.Cashier,
            RoleConstants.InventoryClerk,
            RoleConstants.BranchManager
        };

        foreach (var roleName in defaultRoles)
        {
            if (!existingRoleNames.Contains(roleName))
            {
                context.UserRoles.Add(new UserRole
                {
                    RoleName = roleName,
                    Description = $"{roleName} role",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedDefaultAdminAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        var adminExists = await context.SystemUsers.AnyAsync(u => u.Username == "admin");
        if (adminExists)
        {
            return;
        }

        var adminRole = await context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == RoleConstants.Admin);

        context.SystemUsers.Add(new SystemUser
        {
            UserCode = "USR00001",
            Username = "admin",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            FullName = "System Administrator",
            Email = "admin@possystem.local",
            RoleId = adminRole?.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }
}
