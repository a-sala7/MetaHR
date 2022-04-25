using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class ChangeEmployeeWrittenByToAuthor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeNotes_Employees_EmployeeWrittenAboutId",
                table: "EmployeeNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeNotes_Employees_EmployeeWrittenById",
                table: "EmployeeNotes");

            migrationBuilder.RenameColumn(
                name: "EmployeeWrittenById",
                table: "EmployeeNotes",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "EmployeeWrittenAboutId",
                table: "EmployeeNotes",
                newName: "AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeNotes_EmployeeWrittenById",
                table: "EmployeeNotes",
                newName: "IX_EmployeeNotes_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeNotes_EmployeeWrittenAboutId",
                table: "EmployeeNotes",
                newName: "IX_EmployeeNotes_AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeNotes_Employees_AuthorId",
                table: "EmployeeNotes",
                column: "AuthorId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeNotes_Employees_EmployeeId",
                table: "EmployeeNotes",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeNotes_Employees_AuthorId",
                table: "EmployeeNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeNotes_Employees_EmployeeId",
                table: "EmployeeNotes");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "EmployeeNotes",
                newName: "EmployeeWrittenById");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "EmployeeNotes",
                newName: "EmployeeWrittenAboutId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeNotes_EmployeeId",
                table: "EmployeeNotes",
                newName: "IX_EmployeeNotes_EmployeeWrittenById");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeNotes_AuthorId",
                table: "EmployeeNotes",
                newName: "IX_EmployeeNotes_EmployeeWrittenAboutId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeNotes_Employees_EmployeeWrittenAboutId",
                table: "EmployeeNotes",
                column: "EmployeeWrittenAboutId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeNotes_Employees_EmployeeWrittenById",
                table: "EmployeeNotes",
                column: "EmployeeWrittenById",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
