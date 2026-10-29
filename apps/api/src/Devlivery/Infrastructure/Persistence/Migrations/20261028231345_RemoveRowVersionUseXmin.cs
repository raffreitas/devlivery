using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRowVersionUseXmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
