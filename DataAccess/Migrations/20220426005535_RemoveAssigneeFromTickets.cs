using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class RemoveAssigneeFromTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Employees_AssigneeId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AssigneeId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AssigneeId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Tickets");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssigneeId",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Tickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssigneeId",
                table: "Tickets",
                column: "AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Employees_AssigneeId",
                table: "Tickets",
                column: "AssigneeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
