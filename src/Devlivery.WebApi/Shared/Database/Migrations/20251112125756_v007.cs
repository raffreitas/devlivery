using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.WebApi.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class v007 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "users",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "products",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "order_items",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_users_establishment_id_email",
                table: "users",
                columns: new[] { "establishment_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_establishment_id",
                table: "products",
                column: "establishment_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_establishment_id",
                table: "orders",
                column: "establishment_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_establishment_id",
                table: "order_items",
                column: "establishment_id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_establishments_establishment_id",
                table: "order_items",
                column: "establishment_id",
                principalTable: "establishments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_establishments_establishment_id",
                table: "orders",
                column: "establishment_id",
                principalTable: "establishments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_products_establishments_establishment_id",
                table: "products",
                column: "establishment_id",
                principalTable: "establishments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_users_establishments_establishment_id",
                table: "users",
                column: "establishment_id",
                principalTable: "establishments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_items_establishments_establishment_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_establishments_establishment_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "fk_products_establishments_establishment_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "fk_users_establishments_establishment_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_establishment_id_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_products_establishment_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_orders_establishment_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_order_items_establishment_id",
                table: "order_items");

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "establishment_id",
                table: "order_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
