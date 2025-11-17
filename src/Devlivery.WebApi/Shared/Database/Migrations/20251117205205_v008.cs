using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.WebApi.Shared.Database.Migrations
{
    /// <inheritdoc />
    public partial class v008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                             update public.orders
                             set status = UPPER(LEFT(status, 1)) || LOWER(SUBSTRING(status FROM 2));

                             update public.orders
                             set status = 'Canceled'
                             where status = 'Cancelled';
                             """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                        """
                         UPDATE public.orders
                         SET status = 'cancelled'
                         WHERE status = 'Canceled';
                         
                         UPDATE public.orders
                         SET status = LOWER(status);
                         
                        """);
        }
    }
}
