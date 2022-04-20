using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class ResetDateHiredUtcToDateHired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateHiredUtc",
                table: "Employees",
                newName: "DateHired");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateHired",
                table: "Employees",
                newName: "DateHiredUtc");
        }
    }
}
