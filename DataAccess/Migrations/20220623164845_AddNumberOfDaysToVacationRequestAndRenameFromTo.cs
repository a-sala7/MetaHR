using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddNumberOfDaysToVacationRequestAndRenameFromTo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "To",
                table: "VacationRequests",
                newName: "ToDate");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "VacationRequests",
                newName: "FromDate");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfDays",
                table: "VacationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfDays",
                table: "VacationRequests");

            migrationBuilder.RenameColumn(
                name: "ToDate",
                table: "VacationRequests",
                newName: "To");

            migrationBuilder.RenameColumn(
                name: "FromDate",
                table: "VacationRequests",
                newName: "From");
        }
    }
}
