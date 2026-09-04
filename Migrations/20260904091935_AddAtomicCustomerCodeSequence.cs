using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAtomicCustomerCodeSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "customer_code_sequence");

            // Continue after the highest code already present in production data.
            migrationBuilder.Sql("""
                DECLARE @nextCustomerNumber int =
                    COALESCE((
                        SELECT MAX(TRY_CONVERT(int, SUBSTRING(customer_code, 4, 47))) + 1
                        FROM customer
                        WHERE customer_code LIKE 'CUS%'
                    ), 1);
                DECLARE @restartSql nvarchar(200) =
                    N'ALTER SEQUENCE dbo.customer_code_sequence RESTART WITH '
                    + CONVERT(nvarchar(20), @nextCustomerNumber) + N';';
                EXEC sys.sp_executesql @restartSql;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "customer_code_sequence");
        }
    }
}
