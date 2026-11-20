using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCashWithdrawalRequestId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "request_id",
                table: "cash_session_movements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_cash_session_movements_unique_request",
                table: "cash_session_movements",
                columns: new[] { "establishment_id", "cash_session_id", "request_id" },
                unique: true,
                filter: " \"request_id\" IS NOT NULL ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_cash_session_movements_unique_request",
                table: "cash_session_movements");

            migrationBuilder.DropColumn(
                name: "request_id",
                table: "cash_session_movements");
        }
    }
}
