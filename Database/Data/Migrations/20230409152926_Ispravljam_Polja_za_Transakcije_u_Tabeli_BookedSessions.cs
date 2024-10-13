using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Ispravljam_Polja_za_Transakcije_u_Tabeli_BookedSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "BookedSessions");

            migrationBuilder.AddColumn<string>(
                name: "ClientBookingTransactionId",
                table: "BookedSessions",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TherapistPaymentTransactionId",
                table: "BookedSessions",
                maxLength: 450,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
