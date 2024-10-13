using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Menjam_Velicinu_Comment_u_Tabelama_Ratings_i_PendingRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Ratings",
                maxLength: 768,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "PendingRatings",
                maxLength: 768,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
