using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Kolone_za_Info_u_Tabelu_TherapistPsychotherapyTechniques : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FromDate",
                table: "TherapistPsychotherapyTechniques",
                nullable: true,
                maxLength: 128);

            migrationBuilder.AddColumn<DateTime>(
                name: "ToDate",
                table: "TherapistPsychotherapyTechniques",
                nullable: true,
                maxLength: 128);

            migrationBuilder.AddColumn<bool>(
                name: "Present",
                table: "TherapistPsychotherapyTechniques",
                nullable: true,
                maxLength: 128,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "TherapistPsychotherapyTechniques",
                nullable: true,
                maxLength: 128,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "TherapistPsychotherapyTechniques",
                nullable: true,
                maxLength: 128,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TherapistPsychotherapyTechniques",
                nullable: true,
                maxLength: 2048,
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
