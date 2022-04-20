using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class RenameDateHiredDateRegisteredToUtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateHired",
                table: "Employees",
                newName: "DateHiredUtc");

            migrationBuilder.RenameColumn(
                name: "DateRegistered",
                table: "AspNetUsers",
                newName: "DateRegisteredUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateHiredUtc",
                table: "Employees",
                newName: "DateHired");

            migrationBuilder.RenameColumn(
                name: "DateRegisteredUtc",
                table: "AspNetUsers",
                newName: "DateRegistered");
        }
    }
}
