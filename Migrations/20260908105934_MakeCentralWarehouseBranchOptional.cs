using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class MakeCentralWarehouseBranchOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_stock_inventory_item_code_branch_code_warehouse_code",
                table: "stock_inventory");

            migrationBuilder.AlterColumn<string>(
                name: "branch_code",
                table: "stock_inventory",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_stock_inventory_item_code_branch_code_warehouse_code",
                table: "stock_inventory",
                columns: new[] { "item_code", "branch_code", "warehouse_code" },
                unique: true,
                filter: "[branch_code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_stock_inventory_item_code_branch_code_warehouse_code",
                table: "stock_inventory");

            migrationBuilder.AlterColumn<string>(
                name: "branch_code",
                table: "stock_inventory",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_inventory_item_code_branch_code_warehouse_code",
                table: "stock_inventory",
                columns: new[] { "item_code", "branch_code", "warehouse_code" },
                unique: true);
        }
    }
}
