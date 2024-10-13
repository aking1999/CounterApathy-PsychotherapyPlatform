using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Menjam_Tabele_StripeCustomers_i_StripePaymentIntents_PhoneNumber_u_Nullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "StripeCustomers",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "StripePaymentIntents",
                nullable: true,
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
