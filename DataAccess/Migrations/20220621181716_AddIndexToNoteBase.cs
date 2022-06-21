using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddIndexToNoteBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationNotes_CreatedAtUtc",
                table: "JobApplicationNotes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeNotes_CreatedAtUtc",
                table: "EmployeeNotes",
                column: "CreatedAtUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplicationNotes_CreatedAtUtc",
                table: "JobApplicationNotes");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeNotes_CreatedAtUtc",
                table: "EmployeeNotes");
        }
    }
}
