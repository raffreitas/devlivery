using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.WebApi.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class v013 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE cash_sessions 
                  ALTER COLUMN payment_breakdown TYPE jsonb 
                  USING payment_breakdown::jsonb;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE cash_sessions 
                  ALTER COLUMN payment_breakdown TYPE text 
                  USING payment_breakdown::text;");
        }
    }
}
