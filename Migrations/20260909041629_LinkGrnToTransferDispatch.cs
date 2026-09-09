using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class LinkGrnToTransferDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "dispatch_id",
                table: "grn_master",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "dispatch_line_id",
                table: "grn_item",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_grn_master_dispatch_id",
                table: "grn_master",
                column: "dispatch_id");

            migrationBuilder.CreateIndex(
                name: "IX_grn_item_dispatch_line_id",
                table: "grn_item",
                column: "dispatch_line_id");

            migrationBuilder.AddForeignKey(
                name: "FK_grn_item_stock_transfer_dispatch_line_dispatch_line_id",
                table: "grn_item",
                column: "dispatch_line_id",
                principalTable: "stock_transfer_dispatch_line",
                principalColumn: "dispatch_line_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_grn_master_stock_transfer_dispatch_dispatch_id",
                table: "grn_master",
                column: "dispatch_id",
                principalTable: "stock_transfer_dispatch",
                principalColumn: "dispatch_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_grn_item_stock_transfer_dispatch_line_dispatch_line_id",
                table: "grn_item");

            migrationBuilder.DropForeignKey(
                name: "FK_grn_master_stock_transfer_dispatch_dispatch_id",
                table: "grn_master");

            migrationBuilder.DropIndex(
                name: "IX_grn_master_dispatch_id",
                table: "grn_master");

            migrationBuilder.DropIndex(
                name: "IX_grn_item_dispatch_line_id",
                table: "grn_item");

            migrationBuilder.DropColumn(
                name: "dispatch_id",
                table: "grn_master");

            migrationBuilder.DropColumn(
                name: "dispatch_line_id",
                table: "grn_item");
        }
    }
}
