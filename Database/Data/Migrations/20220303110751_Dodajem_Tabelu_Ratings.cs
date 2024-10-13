using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_Ratings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                 name: "Ratings",
                 columns: table => new
                 {
                     Id = table.Column<string>(maxLength: 450, nullable: false),
                     TherapistId = table.Column<string>(maxLength: 450, nullable: false),
                     ClientId = table.Column<string>(maxLength: 450, nullable: false),
                     ClientFirstName = table.Column<string>(maxLength: 4, nullable: true),
                     ClientLastName = table.Column<string>(maxLength: 4, nullable: true),
                     SessionId = table.Column<string>(maxLength: 450, nullable: false),
                     BookedSessionId = table.Column<string>(maxLength: 450, nullable: false),
                     Rating = table.Column<double>(maxLength: 32, nullable: false, defaultValue: 5),
                     Comment = table.Column<string>(maxLength: 256, nullable: true),
                     RatingDate = table.Column<DateTime>(maxLength: 128, nullable: true),
                     AdminIdWhoApproved = table.Column<string>(maxLength: 450, nullable: false),
                     ApprovalDate = table.Column<DateTime>(maxLength: 128, nullable: true)
                 },
                 constraints: table =>
                 {
                     table.PrimaryKey("PK_Ratings", x => x.Id);
                 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
