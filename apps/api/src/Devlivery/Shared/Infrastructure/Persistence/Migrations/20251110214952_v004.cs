using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class v004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "unit_price",
                table: "order_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                                 UPDATE order_items oi
                                 SET unit_price = p.price
                                 FROM products p
                                 WHERE p.id = oi.product_id;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unit_price",
                table: "order_items");
        }
    }
}