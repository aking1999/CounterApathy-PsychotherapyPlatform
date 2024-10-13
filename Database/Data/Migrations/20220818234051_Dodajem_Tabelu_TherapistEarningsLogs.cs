using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_TherapistEarningsLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TherapistEarningsLogs",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    TherapistId = table.Column<string>(maxLength: 450, nullable: true),
                    Amount = table.Column<double>(maxLength: 32, nullable: false, defaultValue: 0),
                    EarningsDateTime = table.Column<DateTime>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapistEarningsLogs", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistEarningsLogs_Therapists",
                table: "TherapistEarningsLogs",
                column: "TherapistId",
                principalTable: "Therapists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
