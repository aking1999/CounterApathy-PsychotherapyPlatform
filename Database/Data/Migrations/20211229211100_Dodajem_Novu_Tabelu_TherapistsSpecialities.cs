using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Novu_Tabelu_TherapistsSpecialities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TherapistsSpecialities",
                columns: table => new
                {
                    TherapistId = table.Column<string>(maxLength: 450, nullable: false),
                    SpecialityId = table.Column<string>(maxLength: 450, nullable: false)

                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapistsSpecialities", x => new { x.TherapistId, x.SpecialityId });
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistsSpecialities_Therapists",
                table: "TherapistsSpecialities",
                column: "TherapistId",
                principalTable: "Therapists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistsSpecialities_Specialities",
                table: "TherapistsSpecialities",
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
