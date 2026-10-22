using BuildConnect.DAL;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildConnect.DAL.Migrations
{
    [DbContext(typeof(BuildConnectDbContext))]
    [Migration("20260429194500_AddJobLifecycleStatus")]
    public partial class AddJobLifecycleStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "OPEN");

            migrationBuilder.Sql("""
                UPDATE Jobs
                SET Status = CASE
                    WHEN EXISTS (SELECT 1 FROM Reviews WHERE Reviews.JobId = Jobs.Id) THEN 'COMPLETED'
                    WHEN EXISTS (SELECT 1 FROM Bids WHERE Bids.JobId = Jobs.Id AND Bids.Status = 'ACCEPTED') THEN 'IN_PROGRESS'
                    ELSE 'OPEN'
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Jobs");
        }
    }
}
