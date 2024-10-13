using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_StripeAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StripeAccount",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    TherapistId = table.Column<string>(maxLength: 450, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeAccount", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_StripeAccount_Therapists",
                table: "StripeAccount",
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
