using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Kolonu_PhoneNumber_u_Tabelu_TherapistApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "TherapistApplications",
                nullable: true,
                maxLength: 64);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
