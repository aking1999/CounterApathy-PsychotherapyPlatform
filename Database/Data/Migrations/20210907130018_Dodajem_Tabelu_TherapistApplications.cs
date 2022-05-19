using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_TherapistApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "TherapistApplications",
               columns: table => new
               {
                   Id = table.Column<string>(maxLength: 450, nullable: false),
                   UserId = table.Column<string>(maxLength: 450, nullable: true),
                   ApplicationDate = table.Column<string>(maxLength: 128, nullable: true),
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_TherapistApplications", x => x.Id);
                   //table.ForeignKey("FK_TherapistApplications", x => x.UserId,);
               });

            migrationBuilder.CreateTable(
                name: "Specialities",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TherapistApplicationsSpecialities",
                columns: table => new
                {
                    TherapistApplicationId = table.Column<string>(maxLength: 450, nullable: false),
                    SpecialityId = table.Column<string>(maxLength: 450, nullable: false)

                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapistApplicationsSpecialities", x => new { x.TherapistApplicationId, x.SpecialityId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
