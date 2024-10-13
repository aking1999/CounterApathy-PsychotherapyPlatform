using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Kolone_za_Info_u_Tabelu_TherapistsSpecialities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FromDate",
                table: "TherapistsSpecialities",
                nullable: true,
                maxLength: 128);

            migrationBuilder.AddColumn<DateTime>(
                name: "ToDate",
                table: "TherapistsSpecialities",
                nullable: true,
                maxLength: 128);

            migrationBuilder.AddColumn<bool>(
                name: "Present",
                table: "TherapistsSpecialities",
                nullable: true,
                maxLength: 128,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "TherapistsSpecialities",
                nullable: true,
                maxLength: 128,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "TherapistsSpecialities",
                nullable: true,
                maxLength: 128,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TherapistsSpecialities",
                nullable: true,
                maxLength: 2048,
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
