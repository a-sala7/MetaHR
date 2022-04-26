using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddSenderIdToMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderId",
                table: "TicketMessages",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_SenderId",
                table: "TicketMessages",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_Employees_SenderId",
                table: "TicketMessages",
                column: "SenderId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_Employees_SenderId",
                table: "TicketMessages");

            migrationBuilder.DropIndex(
                name: "IX_TicketMessages_SenderId",
                table: "TicketMessages");

            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "TicketMessages");
        }
    }
}
