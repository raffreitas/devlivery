using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveCashWithdrawalIdempotencyToApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_idempotency_records",
                columns: table => new
                {
                    establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    scope_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    response_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_idempotency_records", x => new { x.establishment_id, x.operation, x.scope_id, x.request_id });
                    table.ForeignKey(
                        name: "fk_api_idempotency_records_establishments_establishment_id",
                        column: x => x.establishment_id,
                        principalTable: "establishments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Preserve withdrawals recorded before idempotency moved out of the ledger.
            migrationBuilder.Sql("""
                INSERT INTO public.api_idempotency_records (
                    establishment_id, operation, scope_id, request_id, actor_id, request_hash, response_json
                )
                SELECT establishment_id, 'cash-withdrawal', cash_session_id, request_id, created_by,
                       upper(encode(sha256(convert_to(trim_scale(amount)::text || chr(10) || reason, 'UTF8')), 'hex')),
                       jsonb_build_object('id', id, 'amount', amount, 'reason', reason, 'createdBy', created_by,
                           'createdAt', to_char(created_at AT TIME ZONE 'UTC', 'YYYY-MM-DD"T"HH24:MI:SS.US"Z"'))
                FROM public.cash_session_movements
                WHERE entry_type = 'Withdrawal' AND request_id IS NOT NULL;
                """);

            migrationBuilder.DropIndex(
                name: "idx_cash_session_movements_unique_request",
                table: "cash_session_movements");

            migrationBuilder.DropColumn(
                name: "request_id",
                table: "cash_session_movements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "request_id",
                table: "cash_session_movements",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE public.cash_session_movements AS movement
                SET request_id = request.request_id
                FROM public.api_idempotency_records AS request
                WHERE request.operation = 'cash-withdrawal'
                  AND movement.entry_type = 'Withdrawal'
                  AND movement.establishment_id = request.establishment_id
                  AND movement.cash_session_id = request.scope_id
                  AND movement.id = (request.response_json->>'id')::uuid;
                """);

            migrationBuilder.CreateIndex(
                name: "idx_cash_session_movements_unique_request",
                table: "cash_session_movements",
                columns: new[] { "establishment_id", "cash_session_id", "request_id" },
                unique: true,
                filter: " \"request_id\" IS NOT NULL ");

            migrationBuilder.DropTable(
                name: "api_idempotency_records");
        }
    }
}
