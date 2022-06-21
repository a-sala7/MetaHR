using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddCreatedAtToVacationRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VacationRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "VacationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VacationRequests_CreatedAt",
                table: "VacationRequests",
                column: "CreatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VacationRequests_CreatedAt",
                table: "VacationRequests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VacationRequests");

            migrationBuilder.DropColumn(
                name: "State",
                table: "VacationRequests");
        }
    }
}
