using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeSaleCheckoutIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_stock_batch_stock_id",
                table: "stock_batch");

            migrationBuilder.CreateIndex(
                name: "IX_stock_batch_stock_id_status_received_date",
                table: "stock_batch",
                columns: new[] { "stock_id", "status", "received_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_stock_batch_stock_id_status_received_date",
                table: "stock_batch");

            migrationBuilder.CreateIndex(
                name: "IX_stock_batch_stock_id",
                table: "stock_batch",
                column: "stock_id");
        }
    }
}
