using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddEmployeeNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeWrittenById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeWrittenAboutId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeNotes_Employees_EmployeeWrittenAboutId",
                        column: x => x.EmployeeWrittenAboutId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeNotes_Employees_EmployeeWrittenById",
                        column: x => x.EmployeeWrittenById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeNotes_EmployeeWrittenAboutId",
                table: "EmployeeNotes",
                column: "EmployeeWrittenAboutId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeNotes_EmployeeWrittenById",
                table: "EmployeeNotes",
                column: "EmployeeWrittenById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeNotes");
        }
    }
}
