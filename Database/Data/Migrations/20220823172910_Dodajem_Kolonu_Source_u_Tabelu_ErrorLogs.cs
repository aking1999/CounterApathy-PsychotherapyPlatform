using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Kolonu_Source_u_Tabelu_ErrorLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "ErrorLogs",
                nullable: true,
                maxLength: 512);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
