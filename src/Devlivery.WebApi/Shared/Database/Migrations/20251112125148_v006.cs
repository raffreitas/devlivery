using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.WebApi.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class v006 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "establishment_id",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "establishment_id",
                table: "products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "establishment_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "establishment_id",
                table: "order_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "establishments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trade_name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_establishments", x => x.id);
                });
            
            migrationBuilder.Sql(
                """
                INSERT INTO establishments (id, trade_name, is_active, created_at, updated_at)
                VALUES ('00000000-0000-0000-0000-000000000000', 'Default', true, NOW(), NOW());

                UPDATE users SET establishment_id = '00000000-0000-0000-0000-000000000000' WHERE establishment_id = '00000000-0000-0000-0000-000000000000';
                UPDATE products SET establishment_id = '00000000-0000-0000-0000-000000000000' WHERE establishment_id = '00000000-0000-0000-0000-000000000000';
                UPDATE orders SET establishment_id = '00000000-0000-0000-0000-000000000000' WHERE establishment_id = '00000000-0000-0000-0000-000000000000';
                UPDATE order_items SET establishment_id = '00000000-0000-0000-0000-000000000000' WHERE establishment_id = '00000000-0000-0000-0000-000000000000';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "establishments");

            migrationBuilder.DropColumn(
                name: "establishment_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "establishment_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "establishment_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "establishment_id",
                table: "order_items");
        }
    }
}
