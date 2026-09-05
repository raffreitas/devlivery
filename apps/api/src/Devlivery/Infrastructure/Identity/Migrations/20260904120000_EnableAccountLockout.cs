using Devlivery.Infrastructure.Identity.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Devlivery.Infrastructure.Identity.Migrations;

[DbContext(typeof(ApplicationIdentityDbContext))]
[Migration("20260904120000_EnableAccountLockout")]
public sealed class EnableAccountLockout : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("UPDATE identity.asp_net_users SET lockout_enabled = TRUE WHERE lockout_enabled = FALSE;");

    // Account security state cannot safely be reconstructed on rollback.
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
