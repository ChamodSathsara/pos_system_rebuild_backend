using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PosApi.Models.Entities;

namespace PosApi.Data.Configurations;

/// <summary>
/// Table renamed from "user_group" to "user_role" (and group_id -> role_id) throughout the schema.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_role");
        builder.HasKey(x => x.RoleId);
        builder.Property(x => x.RoleId).HasColumnName("role_id").ValueGeneratedOnAdd();
        builder.Property(x => x.RoleName).HasColumnName("role_name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.RoleName).IsUnique();
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permission");
        builder.HasKey(x => x.PermissionId);
        builder.Property(x => x.PermissionId).HasColumnName("permission_id").ValueGeneratedOnAdd();
        builder.Property(x => x.PermissionName).HasColumnName("permission_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);

        builder.HasIndex(x => x.PermissionName).IsUnique();
    }
}

/// <summary>
/// Table renamed from "user_group_permission" to "user_role_permission".
/// </summary>
public class UserRolePermissionConfiguration : IEntityTypeConfiguration<UserRolePermission>
{
    public void Configure(EntityTypeBuilder<UserRolePermission> builder)
    {
        builder.ToTable("user_role_permission");
        builder.HasKey(x => new { x.RoleId, x.PermissionId });
        builder.Property(x => x.RoleId).HasColumnName("role_id");
        builder.Property(x => x.PermissionId).HasColumnName("permission_id");

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRolePermissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.UserRolePermissions)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SystemUserConfiguration : IEntityTypeConfiguration<SystemUser>
{
    public void Configure(EntityTypeBuilder<SystemUser> builder)
    {
        builder.ToTable("system_user");
        builder.HasKey(x => x.UserCode);
        builder.Property(x => x.UserCode).HasColumnName("user_code").HasMaxLength(50);
        builder.Property(x => x.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(100);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
        builder.Property(x => x.Mobile).HasColumnName("mobile").HasMaxLength(20);
        builder.Property(x => x.BranchCode).HasColumnName("branch_code").HasMaxLength(50);
        builder.Property(x => x.RoleId).HasColumnName("role_id");
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.LastLogin).HasColumnName("last_login");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.Username).IsUnique();

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.SystemUsers)
            .HasForeignKey(x => x.BranchCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.SystemUsers)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_token");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserCode).HasColumnName("user_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Token).HasColumnName("token").HasMaxLength(255).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.RevokedAt).HasColumnName("revoked_at");

        builder.HasIndex(x => x.Token).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserCode)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_token");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserCode).HasColumnName("user_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Token).HasColumnName("token").HasMaxLength(255).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(x => x.UsedAt).HasColumnName("used_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => x.Token).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.PasswordResetTokens)
            .HasForeignKey(x => x.UserCode)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
