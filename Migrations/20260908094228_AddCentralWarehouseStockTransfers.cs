using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCentralWarehouseStockTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_central_warehouse",
                table: "warehouse",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "parent_warehouse_code",
                table: "warehouse",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "stock_transfer_request",
                columns: table => new
                {
                    transfer_request_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    source_warehouse_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    destination_warehouse_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    request_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    required_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    requested_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    accepted_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    accepted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfer_request", x => x.transfer_request_id);
                    table.ForeignKey(
                        name: "FK_stock_transfer_request_warehouse_destination_warehouse_code",
                        column: x => x.destination_warehouse_code,
                        principalTable: "warehouse",
                        principalColumn: "warehouse_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_request_warehouse_source_warehouse_code",
                        column: x => x.source_warehouse_code,
                        principalTable: "warehouse",
                        principalColumn: "warehouse_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_transfer_dispatch",
                columns: table => new
                {
                    dispatch_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dispatch_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    transfer_request_id = table.Column<long>(type: "bigint", nullable: false),
                    vehicle_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    driver_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    dispatched_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    dispatched_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfer_dispatch", x => x.dispatch_id);
                    table.ForeignKey(
                        name: "FK_stock_transfer_dispatch_stock_transfer_request_transfer_request_id",
                        column: x => x.transfer_request_id,
                        principalTable: "stock_transfer_request",
                        principalColumn: "transfer_request_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_transfer_request_line",
                columns: table => new
                {
                    transfer_request_line_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    transfer_request_id = table.Column<long>(type: "bigint", nullable: false),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    requested_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    approved_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    dispatched_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    received_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfer_request_line", x => x.transfer_request_line_id);
                    table.ForeignKey(
                        name: "FK_stock_transfer_request_line_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_request_line_stock_transfer_request_transfer_request_id",
                        column: x => x.transfer_request_id,
                        principalTable: "stock_transfer_request",
                        principalColumn: "transfer_request_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_transfer_receipt",
                columns: table => new
                {
                    receipt_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receipt_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    dispatch_id = table.Column<long>(type: "bigint", nullable: false),
                    received_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    received_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfer_receipt", x => x.receipt_id);
                    table.ForeignKey(
                        name: "FK_stock_transfer_receipt_stock_transfer_dispatch_dispatch_id",
                        column: x => x.dispatch_id,
                        principalTable: "stock_transfer_dispatch",
                        principalColumn: "dispatch_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_transfer_dispatch_line",
                columns: table => new
                {
                    dispatch_line_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dispatch_id = table.Column<long>(type: "bigint", nullable: false),
                    transfer_request_line_id = table.Column<long>(type: "bigint", nullable: false),
                    batch_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfer_dispatch_line", x => x.dispatch_line_id);
                    table.ForeignKey(
                        name: "FK_stock_transfer_dispatch_line_stock_batch_batch_id",
                        column: x => x.batch_id,
                        principalTable: "stock_batch",
                        principalColumn: "batch_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_dispatch_line_stock_transfer_dispatch_dispatch_id",
                        column: x => x.dispatch_id,
                        principalTable: "stock_transfer_dispatch",
                        principalColumn: "dispatch_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stock_transfer_dispatch_line_stock_transfer_request_line_transfer_request_line_id",
                        column: x => x.transfer_request_line_id,
                        principalTable: "stock_transfer_request_line",
                        principalColumn: "transfer_request_line_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_transfer_receipt_line",
                columns: table => new
                {
                    receipt_line_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receipt_id = table.Column<long>(type: "bigint", nullable: false),
                    dispatch_line_id = table.Column<long>(type: "bigint", nullable: false),
                    received_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    short_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    damaged_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfer_receipt_line", x => x.receipt_line_id);
                    table.ForeignKey(
                        name: "FK_stock_transfer_receipt_line_stock_transfer_dispatch_line_dispatch_line_id",
                        column: x => x.dispatch_line_id,
                        principalTable: "stock_transfer_dispatch_line",
                        principalColumn: "dispatch_line_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_receipt_line_stock_transfer_receipt_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "stock_transfer_receipt",
                        principalColumn: "receipt_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_parent_warehouse_code",
                table: "warehouse",
                column: "parent_warehouse_code");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_dispatch_dispatch_no",
                table: "stock_transfer_dispatch",
                column: "dispatch_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_dispatch_transfer_request_id",
                table: "stock_transfer_dispatch",
                column: "transfer_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_dispatch_line_batch_id",
                table: "stock_transfer_dispatch_line",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_dispatch_line_dispatch_id",
                table: "stock_transfer_dispatch_line",
                column: "dispatch_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_dispatch_line_transfer_request_line_id",
                table: "stock_transfer_dispatch_line",
                column: "transfer_request_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_receipt_dispatch_id",
                table: "stock_transfer_receipt",
                column: "dispatch_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_receipt_receipt_no",
                table: "stock_transfer_receipt",
                column: "receipt_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_receipt_line_dispatch_line_id",
                table: "stock_transfer_receipt_line",
                column: "dispatch_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_receipt_line_receipt_id",
                table: "stock_transfer_receipt_line",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_request_destination_warehouse_code_status_request_date",
                table: "stock_transfer_request",
                columns: new[] { "destination_warehouse_code", "status", "request_date" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_request_request_no",
                table: "stock_transfer_request",
                column: "request_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_request_source_warehouse_code_status_request_date",
                table: "stock_transfer_request",
                columns: new[] { "source_warehouse_code", "status", "request_date" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_request_line_item_code",
                table: "stock_transfer_request_line",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_request_line_transfer_request_id",
                table: "stock_transfer_request_line",
                column: "transfer_request_id");

            migrationBuilder.AddForeignKey(
                name: "FK_warehouse_warehouse_parent_warehouse_code",
                table: "warehouse",
                column: "parent_warehouse_code",
                principalTable: "warehouse",
                principalColumn: "warehouse_code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_warehouse_warehouse_parent_warehouse_code",
                table: "warehouse");

            migrationBuilder.DropTable(
                name: "stock_transfer_receipt_line");

            migrationBuilder.DropTable(
                name: "stock_transfer_dispatch_line");

            migrationBuilder.DropTable(
                name: "stock_transfer_receipt");

            migrationBuilder.DropTable(
                name: "stock_transfer_request_line");

            migrationBuilder.DropTable(
                name: "stock_transfer_dispatch");

            migrationBuilder.DropTable(
                name: "stock_transfer_request");

            migrationBuilder.DropIndex(
                name: "IX_warehouse_parent_warehouse_code",
                table: "warehouse");

            migrationBuilder.DropColumn(
                name: "is_central_warehouse",
                table: "warehouse");

            migrationBuilder.DropColumn(
                name: "parent_warehouse_code",
                table: "warehouse");
        }
    }
}
