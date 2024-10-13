using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_ForeignKeyeve_u_Tabele_sa_Uplatama_i_Transakcijama : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_TherapistEarningsLogs_PaymentTypes",
                table: "TherapistEarningsLogs",
                column: "PaymentTypeId",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistEarningsLogs_Transactions",
                table: "TherapistEarningsLogs",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Withdrawals_PaymentTypes",
                table: "Withdrawals",
                column: "PaymentTypeId",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Withdrawals",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Withdrawals_Transactions",
                table: "Withdrawals",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
