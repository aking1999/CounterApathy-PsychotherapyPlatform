using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Povecavam_Velicinu_Kolone_Title_u_Notification_na_1024 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                maxLength: 1024);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
