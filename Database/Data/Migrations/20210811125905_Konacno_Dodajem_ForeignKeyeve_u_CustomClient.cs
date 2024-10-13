using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Konacno_Dodajem_ForeignKeyeve_u_CustomClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Survey",
                table: "AspNetUsers",
                column: "SurveyId",
                principalTable: "Survey",
                principalColumn: "Id"
                );

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistAccount",
                table: "AspNetUsers",
                column: "TherapistAccountId",
                principalTable: "Therapists",
                principalColumn: "Id"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
