using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddTimeIndexToTicketsAndTicketMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedAt",
                table: "Tickets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TimestampUtc",
                table: "TicketMessages",
                column: "TimestampUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_CreatedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_TicketMessages_TimestampUtc",
                table: "TicketMessages");
        }
    }
}
