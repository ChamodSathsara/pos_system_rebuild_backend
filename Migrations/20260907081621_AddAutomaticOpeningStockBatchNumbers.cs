using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomaticOpeningStockBatchNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "opening_stock_batch_sequence");

            migrationBuilder.Sql("""
                DECLARE @nextBatchNumber int =
                    COALESCE((
                        SELECT MAX(TRY_CONVERT(int, SUBSTRING(batch_no, 4, 47))) + 1
                        FROM stock_batch
                        WHERE batch_no LIKE 'BAT%'
                    ), 1);
                DECLARE @restartSql nvarchar(200) =
                    N'ALTER SEQUENCE dbo.opening_stock_batch_sequence RESTART WITH '
                    + CONVERT(nvarchar(20), @nextBatchNumber) + N';';
                EXEC sys.sp_executesql @restartSql;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "opening_stock_batch_sequence");
        }
    }
}
