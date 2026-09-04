using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDamageItemExpenseLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "damage_id",
                table: "expense",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_expense_damage_id",
                table: "expense",
                column: "damage_id",
                unique: true,
                filter: "[damage_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_expense_damage_item_damage_id",
                table: "expense",
                column: "damage_id",
                principalTable: "damage_item",
                principalColumn: "damage_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM expense_category WHERE category_name = 'Damaged Stock')
                BEGIN
                    INSERT INTO expense_category (category_name, description)
                    VALUES ('Damaged Stock', 'Automatically generated expenses for damaged stock.');
                END;

                INSERT INTO expense
                    (damage_id, branch_code, category_id, amount, expense_date, description, paid_by, created_at)
                SELECT
                    d.damage_id,
                    d.branch_code,
                    c.category_id,
                    d.cost_amount,
                    CAST(COALESCE(d.damage_date, SYSUTCDATETIME()) AS date),
                    LEFT(CONCAT('Damage DMG-', d.damage_id, ': ', d.item_code, ' - ', COALESCE(d.reason, '')), 255),
                    d.reported_by,
                    SYSUTCDATETIME()
                FROM damage_item d
                CROSS APPLY (
                    SELECT TOP (1) category_id
                    FROM expense_category
                    WHERE category_name = 'Damaged Stock'
                    ORDER BY category_id
                ) c
                WHERE d.cost_amount > 0
                  AND NOT EXISTS (SELECT 1 FROM expense e WHERE e.damage_id = d.damage_id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM expense WHERE damage_id IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_expense_damage_item_damage_id",
                table: "expense");

            migrationBuilder.DropIndex(
                name: "IX_expense_damage_id",
                table: "expense");

            migrationBuilder.DropColumn(
                name: "damage_id",
                table: "expense");
        }
    }
}
