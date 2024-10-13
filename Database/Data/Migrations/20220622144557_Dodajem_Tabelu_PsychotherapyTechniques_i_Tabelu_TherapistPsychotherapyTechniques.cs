using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_PsychotherapyTechniques_i_Tabelu_TherapistPsychotherapyTechniques : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
              name: "PsychotherapyTechniques",
              columns: table => new
              {
                  Id = table.Column<string>(maxLength: 450, nullable: false),
                  Name = table.Column<string>(maxLength: 128, nullable: true),
                  Color = table.Column<string>(maxLength: 32, nullable: true),
                  Icon = table.Column<string>(maxLength: 32, nullable: true)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_PsychotherapyTechniques", x => x.Id);
              });

            migrationBuilder.CreateTable(
               name: "TherapistPsychotherapyTechniques",
               columns: table => new
               {
                   TherapistId = table.Column<string>(maxLength: 450, nullable: false),
                   PsychotherapyTechniqueId = table.Column<string>(maxLength: 450, nullable: false)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_TherapistPsychotherapyTechniques", x => new { x.TherapistId, x.PsychotherapyTechniqueId });
               });

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistPsychotherapyTechniques_Therapists",
                table: "TherapistPsychotherapyTechniques",
                column: "TherapistId",
                principalTable: "Therapists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistPsychotherapyTechniques_PsychotherapyTechniques",
                table: "TherapistPsychotherapyTechniques",
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
