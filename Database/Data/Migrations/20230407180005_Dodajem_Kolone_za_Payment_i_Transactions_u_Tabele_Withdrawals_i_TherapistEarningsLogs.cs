using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Kolone_za_Payment_i_Transactions_u_Tabele_Withdrawals_i_TherapistEarningsLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentTypeId",
                table: "Withdrawals",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTypeName",
                table: "Withdrawals",
                maxLength: 128,
                nullable: true);


            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "TherapistEarningsLogs",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTypeId",
                table: "TherapistEarningsLogs",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTypeName",
                table: "TherapistEarningsLogs",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
