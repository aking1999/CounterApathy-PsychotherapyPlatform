using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Color_i_Icon_u_Tabelu_Specialities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Specialities",
                maxLength: 32,
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Specialities",
                maxLength: 32,
                nullable: true,
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
