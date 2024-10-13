using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_ErrorLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    UserIdOrAnonymous = table.Column<string>(maxLength: 450, nullable: true),
                    AreaOrProject = table.Column<string>(maxLength: 64, nullable: true),
                    ControllerOrClass = table.Column<string>(maxLength: 64, nullable: true),
                    ActionOrMethod = table.Column<string>(maxLength: 64, nullable: true),
                    Description = table.Column<string>(maxLength: 1024, nullable: true),
                    ErrorDateTime = table.Column<DateTime>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
