using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class MakeBranchNameUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE branch
                SET branch_name = LTRIM(RTRIM(branch_name));

                IF EXISTS (
                    SELECT branch_name
                    FROM branch
                    GROUP BY branch_name
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 50001, 'Duplicate branch names exist. Rename them before applying MakeBranchNameUnique.', 1;
                END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_branch_branch_name",
                table: "branch",
                column: "branch_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_branch_branch_name",
                table: "branch");
        }
    }
}
