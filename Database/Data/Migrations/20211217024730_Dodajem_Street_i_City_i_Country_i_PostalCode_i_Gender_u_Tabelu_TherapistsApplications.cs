using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Street_i_City_i_Country_i_PostalCode_i_Gender_u_Tabelu_TherapistsApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
            name: "Street",
            table: "TherapistApplications",
            maxLength: 128,
            nullable: true,
            defaultValue: null);

            migrationBuilder.AddColumn<string>(
            name: "City",
            table: "TherapistApplications",
            maxLength: 128,
            nullable: true,
            defaultValue: null);

            migrationBuilder.AddColumn<string>(
            name: "Country",
            table: "TherapistApplications",
            maxLength: 128,
            nullable: true,
            defaultValue: null);

            migrationBuilder.AddColumn<string>(
            name: "PostalCode",
            table: "TherapistApplications",
            maxLength: 64,
            nullable: true,
            defaultValue: null);

            migrationBuilder.AddColumn<string>(
            name: "Gender",
            table: "TherapistApplications",
            maxLength: 30,
            nullable: true,
            defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
