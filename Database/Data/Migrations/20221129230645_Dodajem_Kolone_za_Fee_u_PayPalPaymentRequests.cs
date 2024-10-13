using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Kolone_za_Fee_u_PayPalPaymentRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FeePercentage",
                table: "PayPalPaymentRequests",
                maxLength: 64,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PrimaryCurrencyFeeAmount",
                table: "PayPalPaymentRequests",
                maxLength: 64,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "SecondaryCurrencyFeeAmount",
                table: "PayPalPaymentRequests",
                maxLength: 64,
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
