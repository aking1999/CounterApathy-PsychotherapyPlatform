using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_UserAccountInformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccountInformation",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    UserId = table.Column<string>(maxLength: 450, nullable: false),
                    LastActivity = table.Column<string>(maxLength: 256, nullable: true),
                    LastActivityDateTime = table.Column<DateTime>(maxLength: 128, nullable: true),
                    CurrentIpAddress = table.Column<string>(maxLength: 256, nullable: true),
                    SignInDateTime = table.Column<DateTime>(maxLength: 128, nullable: true),
                    RegistrationDateTime = table.Column<DateTime>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountInformation", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccountInformation_AspNetUsers",
                table: "UserAccountInformation",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
