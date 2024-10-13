using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Brisem_Kolonu_SessionDate_u_Tabeli_BookedSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionDate",
                table: "BookedSessions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
