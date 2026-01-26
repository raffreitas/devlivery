using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class v014 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "delivery_reference",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delivery_reference",
                table: "orders");
        }
    }
}