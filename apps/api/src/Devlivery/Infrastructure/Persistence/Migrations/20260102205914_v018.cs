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
            // =========================================================================
            // STEP 1: Backup existing payment data
            // =========================================================================
            migrationBuilder.Sql(@"
                -- Create backup table for payment data migration
                DROP TABLE IF EXISTS temp_orders_backup_v018;
                
                CREATE TABLE temp_orders_backup_v018 AS
                SELECT 
                    id,
                    establishment_id,
                    payment_method,
                    total,
                    status,
                    created_at,
                    updated_at
                FROM orders
                WHERE payment_method IS NOT NULL;
                
                -- Create indexes for faster migration
                CREATE INDEX idx_temp_orders_backup_v018_id ON temp_orders_backup_v018(id);
                CREATE INDEX idx_temp_orders_backup_v018_establishment ON temp_orders_backup_v018(establishment_id);
            ");

            // =========================================================================
            // STEP 2: Drop old structures
            // =========================================================================
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

            // =========================================================================
            // STEP 3: Migrate payment data from backup to order_payments
            // =========================================================================
            migrationBuilder.Sql(@"
                -- Migrate historical payment data to new order_payments table
                INSERT INTO order_payments (
                    id,
                    establishment_id,
                    payment_method,
                    confirmed_at,
                    payment_status,
                    amount,
                    created_at,
                    updated_at,
                    order_id
                )
                SELECT 
                    gen_random_uuid() as id,
                    t.establishment_id,
                    t.payment_method,
                    t.updated_at as confirmed_at,
                    'Confirmed' as payment_status,
                    t.total as amount,
                    t.created_at,
                    t.updated_at,
                    t.id as order_id
                FROM temp_orders_backup_v018 t
                WHERE NOT EXISTS (
                    SELECT 1 FROM order_payments op WHERE op.order_id = t.id
                );
                
                -- Set change to 0 for migrated orders (default value)
                UPDATE orders 
                SET change = 0 
                WHERE id IN (SELECT id FROM temp_orders_backup_v018)
                  AND change IS NULL;
            ");

            // =========================================================================
            // STEP 4: Validation - Log migration results
            // =========================================================================
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    backup_count INTEGER;
                    migrated_count INTEGER;
                    orphan_count INTEGER;
                BEGIN
                    SELECT COUNT(*) INTO backup_count FROM temp_orders_backup_v018;
                    
                    SELECT COUNT(*) INTO migrated_count 
                    FROM order_payments op
                    INNER JOIN temp_orders_backup_v018 t ON t.id = op.order_id;
                    
                    SELECT COUNT(*) INTO orphan_count 
                    FROM temp_orders_backup_v018 t
                    WHERE NOT EXISTS (
                        SELECT 1 FROM order_payments op WHERE op.order_id = t.id
                    );
                    
                    RAISE NOTICE '=== Migration v018 Validation ===';
                    RAISE NOTICE 'Orders backed up: %', backup_count;
                    RAISE NOTICE 'Payments migrated: %', migrated_count;
                    RAISE NOTICE 'Orders without payment: %', orphan_count;
                    
                    IF orphan_count > 0 THEN
                        RAISE EXCEPTION 'Migration failed: % orders without payments!', orphan_count;
                    END IF;
                    
                    IF migrated_count != backup_count THEN
                        RAISE EXCEPTION 'Migration validation failed: expected %, got %', backup_count, migrated_count;
                    END IF;
                    
                    RAISE NOTICE 'Migration v018 completed successfully';
                END $$;
            ");

            // =========================================================================
            // STEP 5: Keep backup table for safety (can be dropped later manually)
            // =========================================================================
            // The temp_orders_backup_v018 table is intentionally kept
            // Drop it manually after validating the migration in production
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

            // =========================================================================
            // ROLLBACK: Restore payment data from backup if available
            // =========================================================================
            migrationBuilder.Sql(@"
                -- Restore payment_method from backup if table exists
                DO $$
                BEGIN
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'temp_orders_backup_v018') THEN
                        UPDATE orders o
                        SET payment_method = t.payment_method
                        FROM temp_orders_backup_v018 t
                        WHERE o.id = t.id;
                        
                        RAISE NOTICE 'Payment method data restored from backup';
                    ELSE
                        RAISE WARNING 'Backup table not found - payment_method data cannot be restored';
                    END IF;
                END $$;
            ");

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