using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAtomicBranchCodeSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "branch_code_sequence");

            migrationBuilder.Sql("""
                DECLARE @nextBranchNumber int =
                    COALESCE((
                        SELECT MAX(TRY_CONVERT(int, SUBSTRING(branch_code, 4, 47))) + 1
                        FROM branch
                        WHERE branch_code LIKE 'BRA%'
                    ), 1);
                DECLARE @restartSql nvarchar(200) =
                    N'ALTER SEQUENCE dbo.branch_code_sequence RESTART WITH '
                    + CONVERT(nvarchar(20), @nextBranchNumber) + N';';
                EXEC sys.sp_executesql @restartSql;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "branch_code_sequence");
        }
    }
}
