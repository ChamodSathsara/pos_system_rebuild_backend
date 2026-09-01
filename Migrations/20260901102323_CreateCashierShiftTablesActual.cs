using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateCashierShiftTablesActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cashier_shift",
                columns: table => new
                {
                    shift_id = table.Column<int>(
                            type: "int",
                            nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    branch_code = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: true),

                    cashier_code = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: true),

                    opening_cash = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: false),

                    opened_at = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    expected_cash = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: true),

                    actual_cash = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: true),

                    difference_amount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: true),

                    reason_type = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: true),

                    reason_description = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: true),

                    status = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: false),

                    closed_by = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: true),

                    closed_at = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        name: "PK_cashier_shift",
                        columns: x => x.shift_id);

                    table.ForeignKey(
                        name: "FK_cashier_shift_branch_branch_code",
                        column: x => x.branch_code,
                        principalTable: "branch",
                        principalColumn: "branch_code",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_cashier_shift_system_user_cashier_code",
                        column: x => x.cashier_code,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_cashier_shift_system_user_closed_by",
                        column: x => x.closed_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cashier_shift_history",
                columns: table => new
                {
                    history_id = table.Column<int>(
                            type: "int",
                            nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    shift_id = table.Column<int>(
                        type: "int",
                        nullable: true),

                    action = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: false),

                    expected_cash = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: true),

                    actual_cash = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: true),

                    difference_amount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: true),

                    reason_type = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: true),

                    reason_description = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: true),

                    changed_by = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: true),

                    changed_at = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true),

                    remarks = table.Column<string>(
                        type: "nvarchar(255)",
                        maxLength: 255,
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        name: "PK_cashier_shift_history",
                        columns: x => x.history_id);

                    table.ForeignKey(
                        name: "FK_cashier_shift_history_cashier_shift_shift_id",
                        column: x => x.shift_id,
                        principalTable: "cashier_shift",
                        principalColumn: "shift_id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_cashier_shift_history_system_user_changed_by",
                        column: x => x.changed_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cashier_shift_branch_code",
                table: "cashier_shift",
                column: "branch_code");

            migrationBuilder.CreateIndex(
                name: "IX_cashier_shift_cashier_code",
                table: "cashier_shift",
                column: "cashier_code");

            migrationBuilder.CreateIndex(
                name: "IX_cashier_shift_closed_by",
                table: "cashier_shift",
                column: "closed_by");

            migrationBuilder.CreateIndex(
                name: "IX_cashier_shift_history_shift_id",
                table: "cashier_shift_history",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "IX_cashier_shift_history_changed_by",
                table: "cashier_shift_history",
                column: "changed_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Foreign key dependency නිසා history table එක මුලින් remove කළ යුතුයි.
            migrationBuilder.DropTable(
                name: "cashier_shift_history");

            migrationBuilder.DropTable(
                name: "cashier_shift");
        }
    }
}
