using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_HostedServicesInformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HostedServicesInformation",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    Information = table.Column<string>(maxLength: 450, nullable: false),
                    InformationType = table.Column<string>(maxLength: 128, nullable: false),
                    ExecutionDateTime = table.Column<DateTime>(maxLength: 128, nullable: true),

                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostedServicesInformation", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
