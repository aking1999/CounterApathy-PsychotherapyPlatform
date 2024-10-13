using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Foreign_Keyeve_u_Tabele_TherapistApplications_Prema_AspNetUsers_i_TherapistApplicationsSpecialities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_TherapistApplications_AspNetUsers",
                table: "TherapistApplications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistApplicationsSpecialities_TherapistApplications",
                table: "TherapistApplicationsSpecialities",
                column: "TherapistApplicationId",
                principalTable: "TherapistApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistApplicationsSpecialities_Specialities",
                table: "TherapistApplicationsSpecialities",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
