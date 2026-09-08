using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PosApi.Data;

#nullable disable

namespace PosApi.Migrations;

/// <summary>
/// Corrects the central-warehouse migration: a central warehouse is shared by all
/// branches, therefore warehouse.branch_code must allow NULL.
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260908112000_MakeWarehouseBranchColumnNullable")]
public partial class MakeWarehouseBranchColumnNullable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "branch_code",
            table: "warehouse",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "branch_code",
            table: "warehouse",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50,
            oldNullable: true);
    }
}
