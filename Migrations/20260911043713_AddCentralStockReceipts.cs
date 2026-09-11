using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCentralStockReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "central_stock_receipt_sequence");

            migrationBuilder.CreateTable(
                name: "central_stock_receipt",
                columns: table => new
                {
                    receipt_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receipt_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    warehouse_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    receipt_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reference_no = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    total_quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    total_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    received_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_central_stock_receipt", x => x.receipt_id);
                    table.ForeignKey(
                        name: "FK_central_stock_receipt_system_user_received_by",
                        column: x => x.received_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_central_stock_receipt_warehouse_warehouse_code",
                        column: x => x.warehouse_code,
                        principalTable: "warehouse",
                        principalColumn: "warehouse_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "central_stock_receipt_line",
                columns: table => new
                {
                    receipt_line_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receipt_id = table.Column<long>(type: "bigint", nullable: false),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    batch_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    selling_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_central_stock_receipt_line", x => x.receipt_line_id);
                    table.ForeignKey(
                        name: "FK_central_stock_receipt_line_central_stock_receipt_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "central_stock_receipt",
                        principalColumn: "receipt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_central_stock_receipt_line_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_central_stock_receipt_line_stock_batch_batch_id",
                        column: x => x.batch_id,
                        principalTable: "stock_batch",
                        principalColumn: "batch_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_central_stock_receipt_receipt_no",
                table: "central_stock_receipt",
                column: "receipt_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_central_stock_receipt_received_by",
                table: "central_stock_receipt",
                column: "received_by");

            migrationBuilder.CreateIndex(
                name: "IX_central_stock_receipt_warehouse_code_receipt_date",
                table: "central_stock_receipt",
                columns: new[] { "warehouse_code", "receipt_date" });

            migrationBuilder.CreateIndex(
                name: "IX_central_stock_receipt_line_batch_id",
                table: "central_stock_receipt_line",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_central_stock_receipt_line_item_code",
                table: "central_stock_receipt_line",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_central_stock_receipt_line_receipt_id",
                table: "central_stock_receipt_line",
                column: "receipt_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "central_stock_receipt_line");

            migrationBuilder.DropTable(
                name: "central_stock_receipt");

            migrationBuilder.DropSequence(
                name: "central_stock_receipt_sequence");
        }
    }
}
