using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Therapists_i_TherapistsContactMethods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Therapists",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    TherapistsContactMethodsId = table.Column<string>(maxLength: 450, nullable: true),
                    OnVacation = table.Column<int>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Therapists", x => x.Id);
                    /*table.ForeignKey(
                        name: "FK_ContactMethods",
                        column: x => x.TherapistsContactMethodsId,
                        principalTable: "TherapistsContactMethods",
                        principalColumn: "TherapistId",
                        onDelete: ReferentialAction.Cascade);*/
                });

            migrationBuilder.CreateTable(
                name: "TherapistsContactMethods",
                columns: table => new
                {
                    TherapistId = table.Column<string>(maxLength: 450, nullable: false),
                    ContactMethodId = table.Column<string>(maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapistsContactMethods", x => new { x.TherapistId, x.ContactMethodId });
                    /*table.ForeignKey(
                        name: "FK_TherapistsContactMethods_ContactMethodId",
                        column: x => x.ContactMethodId,
                        principalTable: "ContactMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);*/
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
