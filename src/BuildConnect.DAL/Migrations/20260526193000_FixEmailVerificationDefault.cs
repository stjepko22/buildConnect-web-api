using BuildConnect.DAL;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildConnect.DAL.Migrations
{
    [DbContext(typeof(BuildConnectDbContext))]
    [Migration("20260526193000_FixEmailVerificationDefault")]
    public partial class FixEmailVerificationDefault : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Users]
                SET [IsEmailVerified] = 0
                WHERE [EmailVerificationToken] IS NOT NULL
                  AND [EmailVerificationTokenExpiresAt] IS NOT NULL
                """);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsEmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
