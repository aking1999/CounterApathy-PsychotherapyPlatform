using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Street_City_Country_PostalCode_Gender_About_u_Tabelu_Therapists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Therapists",
                maxLength: 30,
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Therapists",
                maxLength: 128,
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Therapists",
                maxLength: 128,
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Therapists",
                maxLength: 128,
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Therapists",
                maxLength: 64,
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "Therapists",
                maxLength: 1024,
                nullable: true,
                defaultValue: null);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
