using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class dodajem_customclient_i_obrisao_sam_application_db_context : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                nullable: true,
                maxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                nullable: true,
                maxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhoto",
                table: "AspNetUsers",
                nullable: true,
                maxLength: 450);

            migrationBuilder.AddColumn<string>(
                name: "SurveyId",
                table: "AspNetUsers",
                nullable: true,
                maxLength: 450);

            migrationBuilder.AddColumn<string>(
                name: "TherapistAccountId",
                table: "AspNetUsers",
                nullable: true,
                maxLength: 450);

            migrationBuilder.AddColumn<double>(
                name: "WebCredit",
                table: "AspNetUsers",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YearOfBirth",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePhoto",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SurveyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TherapistAccountId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WebCredit",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "YearOfBirth",
                table: "AspNetUsers");
        }
    }
}
