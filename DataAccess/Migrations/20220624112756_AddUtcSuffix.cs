using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddUtcSuffix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "VacationRequests",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_VacationRequests_CreatedAt",
                table: "VacationRequests",
                newName: "IX_VacationRequests_CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Tickets",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_CreatedAt",
                table: "Tickets",
                newName: "IX_Tickets_CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Announcements",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Announcements_CreatedAt",
                table: "Announcements",
                newName: "IX_Announcements_CreatedAtUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "VacationRequests",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_VacationRequests_CreatedAtUtc",
                table: "VacationRequests",
                newName: "IX_VacationRequests_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Tickets",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_CreatedAtUtc",
                table: "Tickets",
                newName: "IX_Tickets_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Announcements",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Announcements_CreatedAtUtc",
                table: "Announcements",
                newName: "IX_Announcements_CreatedAt");
        }
    }
}
