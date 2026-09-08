using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalPurchaseOrderWarehouseLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "destination_warehouse_code",
                table: "purchase_order",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_internal_transfer",
                table: "purchase_order",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "source_warehouse_code",
                table: "purchase_order",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "transfer_request_id",
                table: "purchase_order",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_destination_warehouse_code",
                table: "purchase_order",
                column: "destination_warehouse_code");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_source_warehouse_code",
                table: "purchase_order",
                column: "source_warehouse_code");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_order_warehouse_destination_warehouse_code",
                table: "purchase_order",
                column: "destination_warehouse_code",
                principalTable: "warehouse",
                principalColumn: "warehouse_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_order_warehouse_source_warehouse_code",
                table: "purchase_order",
                column: "source_warehouse_code",
                principalTable: "warehouse",
                principalColumn: "warehouse_code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_order_warehouse_destination_warehouse_code",
                table: "purchase_order");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_order_warehouse_source_warehouse_code",
                table: "purchase_order");

            migrationBuilder.DropIndex(
                name: "IX_purchase_order_destination_warehouse_code",
                table: "purchase_order");

            migrationBuilder.DropIndex(
                name: "IX_purchase_order_source_warehouse_code",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "destination_warehouse_code",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "is_internal_transfer",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "source_warehouse_code",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "transfer_request_id",
                table: "purchase_order");
        }
    }
}
