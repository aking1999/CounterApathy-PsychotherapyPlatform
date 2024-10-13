using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_ContactMethods_i_Survey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "Survey",
               columns: table => new
               {
                   Id = table.Column<string>(maxLength: 450, nullable: false),
                   ListOfProblems = table.Column<string>(maxLength: 256, nullable: true)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_Survey", x => x.Id);
               });

            migrationBuilder.CreateTable(
                name: "ContactMethods",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    Icon = table.Column<string>(maxLength: 450, nullable: true),
                    Name = table.Column<string>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMethods", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
