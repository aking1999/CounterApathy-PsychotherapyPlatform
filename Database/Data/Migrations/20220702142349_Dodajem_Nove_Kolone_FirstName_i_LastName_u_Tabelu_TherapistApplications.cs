using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Nove_Kolone_FirstName_i_LastName_u_Tabelu_TherapistApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "TherapistApplications",
                nullable: true,
                maxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "TherapistApplications",
                nullable: true,
                maxLength: 128);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
