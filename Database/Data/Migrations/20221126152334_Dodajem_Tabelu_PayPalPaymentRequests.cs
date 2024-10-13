using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_PayPalPaymentRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayPalPaymentRequests",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    UserId = table.Column<string>(maxLength: 450, nullable: true),
                    WebCreditLogId = table.Column<string>(maxLength: 450, nullable: true),
                    PayPalPayerId = table.Column<string>(maxLength: 450, nullable: true),
                    PayPalPaymentId = table.Column<string>(maxLength: 450, nullable: true),
                    PayPalTransactionId = table.Column<string>(maxLength: 450, nullable: true),
                    PrimaryCurrencyCode = table.Column<string>(maxLength: 32, nullable: false),
                    SecondaryCurrencyCode = table.Column<string>(maxLength: 32, nullable: false),
                    PrimaryCurrencyAmount = table.Column<double>(maxLength: 64, nullable: false),
                    SecondaryCurrencyAmount = table.Column<double>(maxLength: 64, nullable: false),
                    ExchangeRate = table.Column<double>(maxLength: 64, nullable: false),
                    RequestDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    Status = table.Column<int>(maxLength: 32, nullable: false),
                    PaymentCompleteDateTime = table.Column<DateTime>(maxLength: 128, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayPalPaymentRequests", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PayPalPaymentRequests_AspNetUsers",
                table: "PayPalPaymentRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PayPalPaymentRequests_WebCreditLogs",
                table: "PayPalPaymentRequests",
                column: "WebCreditLogId",
                principalTable: "WebCreditLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
