using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_TherapistApplicationsPsychotherapyTechniques : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TherapistApplicationsPsychotherapyTechniques",
                columns: table => new
                {
                    TherapistApplicationId = table.Column<string>(maxLength: 450, nullable: false),
                    PsychotherapyTechniqueId = table.Column<string>(maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapistApplicationsPsychotherapyTechniques", x => new { x.TherapistApplicationId, x.PsychotherapyTechniqueId });
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistApplicationsPsychotherapyTechniques_TherapistApplications",
                table: "TherapistApplicationsPsychotherapyTechniques",
                column: "TherapistApplicationId",
                principalTable: "TherapistApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistApplicationsPsychotherapyTechniques_PsychotherapyTechniques",
                table: "TherapistApplicationsPsychotherapyTechniques",
                column: "PsychotherapyTechniqueId",
                principalTable: "PsychotherapyTechniques",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
