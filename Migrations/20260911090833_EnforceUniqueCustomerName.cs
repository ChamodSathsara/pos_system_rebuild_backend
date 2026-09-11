using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueCustomerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_name_key",
                table: "customer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_customer_name_key",
                table: "customer",
                column: "customer_name_key",
                unique: true,
                filter: "[customer_name_key] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_customer_customer_name_key",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "customer_name_key",
                table: "customer");
        }
    }
}
