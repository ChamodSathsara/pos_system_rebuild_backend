using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_audit_log_action_time",
                table: "audit_log",
                column: "action_time");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_table_name_action_time",
                table: "audit_log",
                columns: new[] { "table_name", "action_time" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_user_code_action_time",
                table: "audit_log",
                columns: new[] { "user_code", "action_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_audit_log_action_time",
                table: "audit_log");

            migrationBuilder.DropIndex(
                name: "IX_audit_log_table_name_action_time",
                table: "audit_log");

            migrationBuilder.DropIndex(
                name: "IX_audit_log_user_code_action_time",
                table: "audit_log");
        }
    }
}
