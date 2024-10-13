using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_UserAgent_u_Tabele_za_Cuvanje_Aktivnosti : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "UserAccountInformation",
                nullable: true,
                maxLength: 1024);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "UserActivityLogs",
                nullable: true,
                maxLength: 1024);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
