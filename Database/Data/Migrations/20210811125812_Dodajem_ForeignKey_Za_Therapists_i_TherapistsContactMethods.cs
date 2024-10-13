using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_ForeignKey_Za_Therapists_i_TherapistsContactMethods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                        name: "FK_ContactMethods",
                        table: "TherapistsContactMethods",
                        column: "TherapistId",
                        principalTable: "Therapists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                        name: "FK_TherapistsContactMethods_ContactMethodId",
                        table: "TherapistsContactMethods",
                        column: "ContactMethodId",
                        principalTable: "ContactMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
