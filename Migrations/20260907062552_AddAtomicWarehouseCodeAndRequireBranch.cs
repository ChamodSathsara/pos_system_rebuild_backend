using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAtomicWarehouseCodeAndRequireBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "warehouse_code_sequence");

            migrationBuilder.Sql("""
                DECLARE @nextWarehouseNumber int =
                    COALESCE((
                        SELECT MAX(TRY_CONVERT(int, SUBSTRING(warehouse_code, 3, 48))) + 1
                        FROM warehouse
                        WHERE warehouse_code LIKE 'WH%'
                    ), 1);
                DECLARE @restartSql nvarchar(200) =
                    N'ALTER SEQUENCE dbo.warehouse_code_sequence RESTART WITH '
                    + CONVERT(nvarchar(20), @nextWarehouseNumber) + N';';
                EXEC sys.sp_executesql @restartSql;
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM warehouse
                    WHERE branch_code IS NULL OR LTRIM(RTRIM(branch_code)) = ''
                )
                    THROW 51000, 'Every existing warehouse must have a branch before applying this migration.', 1;
                """);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "warehouse_code_sequence");

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
    }
}
