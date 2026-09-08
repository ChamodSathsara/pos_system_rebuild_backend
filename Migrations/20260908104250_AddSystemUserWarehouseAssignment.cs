using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemUserWarehouseAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "warehouse_code",
                table: "system_user",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_system_user_warehouse_code",
                table: "system_user",
                column: "warehouse_code");

            migrationBuilder.AddForeignKey(
                name: "FK_system_user_warehouse_warehouse_code",
                table: "system_user",
                column: "warehouse_code",
                principalTable: "warehouse",
                principalColumn: "warehouse_code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_system_user_warehouse_warehouse_code",
                table: "system_user");

            migrationBuilder.DropIndex(
                name: "IX_system_user_warehouse_code",
                table: "system_user");

            migrationBuilder.DropColumn(
                name: "warehouse_code",
                table: "system_user");
        }
    }
}
