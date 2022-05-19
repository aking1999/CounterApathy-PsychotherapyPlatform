using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_University_i_PastCompanies_u_Tabelu_TherapistApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "University",
                table: "TherapistApplications",
                maxLength: 512,
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "PastCompanies",
                table: "TherapistApplications",
                maxLength: 512,
                nullable: true,
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
