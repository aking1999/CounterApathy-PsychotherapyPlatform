using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Kolone_ContactMethodColor_i_ContactMethodIcon_u_Tabelu_BookedSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactMethodColor",
                table: "BookedSessions",
                maxLength: 64,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "ContactMethodIcon",
                table: "BookedSessions",
                maxLength: 64,
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
