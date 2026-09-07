using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class SecureRefreshTokenRotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_refresh_token_token",
                table: "refresh_token");

            migrationBuilder.DropIndex(
                name: "IX_refresh_token_user_code",
                table: "refresh_token");

            // Existing rows contain raw bearer secrets. Invalidate them instead of carrying
            // plaintext credentials into the new hashed-token design.
            migrationBuilder.Sql("DELETE FROM refresh_token;");

            migrationBuilder.DropColumn(
                name: "token",
                table: "refresh_token");

            migrationBuilder.AddColumn<string>(
                name: "family_id",
                table: "refresh_token",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "replaced_by_token_hash",
                table: "refresh_token",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "token_hash",
                table: "refresh_token",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_token_hash",
                table: "refresh_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_user_code_family_id",
                table: "refresh_token",
                columns: new[] { "user_code", "family_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM refresh_token;");

            migrationBuilder.DropIndex(
                name: "IX_refresh_token_token_hash",
                table: "refresh_token");

            migrationBuilder.DropIndex(
                name: "IX_refresh_token_user_code_family_id",
                table: "refresh_token");

            migrationBuilder.DropColumn(
                name: "family_id",
                table: "refresh_token");

            migrationBuilder.DropColumn(
                name: "replaced_by_token_hash",
                table: "refresh_token");

            migrationBuilder.DropColumn(
                name: "token_hash",
                table: "refresh_token");

            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "refresh_token",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_token",
                table: "refresh_token",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_user_code",
                table: "refresh_token",
                column: "user_code");
        }
    }
}
