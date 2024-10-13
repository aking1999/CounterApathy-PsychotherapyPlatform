using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_PendingRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                 name: "PendingRatings",
                 columns: table => new
                 {
                     Id = table.Column<string>(maxLength: 450, nullable: false),
                     TherapistId = table.Column<string>(maxLength: 450, nullable: false),
                     ClientId = table.Column<string>(maxLength: 450, nullable: false),
                     SessionId = table.Column<string>(maxLength: 450, nullable: false),
                     BookedSessionId = table.Column<string>(maxLength: 450, nullable: false),
                     Rating = table.Column<double>(maxLength: 32, nullable: false, defaultValue: 5),
                     Comment = table.Column<string>(maxLength: 256, nullable: true),
                     RatingDate = table.Column<DateTime>(maxLength: 128, nullable: true)
                 },
                 constraints: table =>
                 {
                     table.PrimaryKey("PK_PendingRatings", x => x.Id);
                 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
        }
    }
}
