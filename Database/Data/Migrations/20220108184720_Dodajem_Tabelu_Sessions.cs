using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_Sessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "Sessions",
               columns: table => new
               {
                   Id = table.Column<string>(maxLength: 450, nullable: false),
                   TherapistId = table.Column<string>(maxLength: 450, nullable: false),
                   Subject = table.Column<string>(maxLength: 64, nullable: true),
                   Description = table.Column<string>(maxLength: 192, nullable: true),
                   Price = table.Column<double>(maxLength: 64, nullable: false, defaultValue: 0),
                   Type = table.Column<int>(maxLength: 16, nullable: false, defaultValue: 0),
                   StartDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                   EndDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                   Color = table.Column<string>(maxLength: 32, nullable: true),
                   Icon = table.Column<string>(maxLength: 32, nullable: true),
                   Booked = table.Column<int>(maxLength: 16, nullable: false, defaultValue: 0)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_Sessions", x => x.Id);
                   table.ForeignKey(
                        name: "FK_Sessions_Therapists",
                        column: x => x.TherapistId,
                        principalTable: "Therapists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
               });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
