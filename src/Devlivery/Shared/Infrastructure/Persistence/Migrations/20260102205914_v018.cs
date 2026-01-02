using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class v018 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cash_deposits");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "expected_cash_amount",
                table: "cash_sessions");

            migrationBuilder.DropColumn(
                name: "payment_breakdown",
                table: "cash_sessions");

            migrationBuilder.DropColumn(
                name: "total_orders",
                table: "cash_sessions");

            migrationBuilder.DropColumn(
                name: "total_revenue",
                table: "cash_sessions");

            migrationBuilder.AddColumn<decimal>(
                name: "change",
                table: "orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "cash_session_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cash_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    related_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash_session_movements", x => x.id);
                    table.ForeignKey(
                        name: "fk_cash_session_movements_cash_sessions_cash_session_id",
                        column: x => x.cash_session_id,
                        principalTable: "cash_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cash_session_movements_establishments_establishment_id",
                        column: x => x.establishment_id,
                        principalTable: "establishments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_payments_establishments_establishment_id",
                        column: x => x.establishment_id,
                        principalTable: "establishments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_payments_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cash_session_movements_cash_session_id",
                table: "cash_session_movements",
                column: "cash_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_cash_session_movements_created_at",
                table: "cash_session_movements",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_cash_session_movements_establishment_id",
                table: "cash_session_movements",
                column: "establishment_id");

            migrationBuilder.CreateIndex(
                name: "ix_cash_session_movements_related_order_id",
                table: "cash_session_movements",
                column: "related_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_payments_establishment_id",
                table: "order_payments",
                column: "establishment_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_payments_order_id",
                table: "order_payments",
                column: "order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cash_session_movements");

            migrationBuilder.DropTable(
                name: "order_payments");

            migrationBuilder.DropColumn(
                name: "change",
                table: "orders");

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "expected_cash_amount",
                table: "cash_sessions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "payment_breakdown",
                table: "cash_sessions",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "total_orders",
                table: "cash_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "total_revenue",
                table: "cash_sessions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "cash_deposits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    attendant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendant_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cash_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deposited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash_deposits", x => x.id);
                    table.ForeignKey(
                        name: "fk_cash_deposits_cash_sessions_cash_session_id",
                        column: x => x.cash_session_id,
                        principalTable: "cash_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cash_deposits_establishments_establishment_id",
                        column: x => x.establishment_id,
                        principalTable: "establishments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cash_deposits_cash_session_id",
                table: "cash_deposits",
                column: "cash_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_cash_deposits_deposited_at",
                table: "cash_deposits",
                column: "deposited_at");

            migrationBuilder.CreateIndex(
                name: "ix_cash_deposits_establishment_id",
                table: "cash_deposits",
                column: "establishment_id");
        }
    }
}
