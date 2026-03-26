using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class v019 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove duplicate cash session movements for the same order payment
            migrationBuilder.Sql("""
                                 WITH duplicates AS (SELECT id,
                                                            ROW_NUMBER()
                                                            OVER (PARTITION BY order_payment_id, cash_session_id, entry_type ORDER BY created_at ASC, id ASC) rn
                                                     FROM cash_session_movements
                                                     WHERE order_payment_id IS NOT NULL
                                                       AND entry_type = 'Payment')
                                 DELETE
                                 FROM cash_session_movements USING duplicates
                                 WHERE cash_session_movements.id = duplicates.id
                                   AND duplicates.rn > 1;
                                 """);

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "orders",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "order_payments",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "cash_sessions",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "cash_session_movements",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_cash_session_movements_unique_payment",
                table: "cash_session_movements",
                columns: new[] { "order_payment_id", "cash_session_id", "entry_type" },
                unique: true,
                filter: "\"order_payment_id\" IS NOT NULL AND \"entry_type\" = 'Payment'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_cash_session_movements_unique_payment",
                table: "cash_session_movements");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "order_payments");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "cash_sessions");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "cash_session_movements");
        }
    }
}