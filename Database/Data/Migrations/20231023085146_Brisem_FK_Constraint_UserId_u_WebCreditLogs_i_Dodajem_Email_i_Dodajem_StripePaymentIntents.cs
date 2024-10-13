using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Brisem_FK_Constraint_UserId_u_WebCreditLogs_i_Dodajem_Email_i_Dodajem_StripePaymentIntents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebCreditLogs_AspNetUsers",
                table: "WebCreditLogs");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "WebCreditLogs",
                maxLength: 256,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "StripeCustomers",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "StripePaymentIntents",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    UserIdOrAnonymous = table.Column<string>(maxLength: 450, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(nullable: false),
                    CustomerId = table.Column<string>(maxLength: 450, nullable: true),
                    Status = table.Column<int>(maxLength: 16, nullable: false),
                    SystemEventTypeName = table.Column<string>(maxLength: 128, nullable: false),
                    StripeEventTypeName = table.Column<string>(maxLength: 128, nullable: false),
                    MustSucceedUntil = table.Column<DateTime>(maxLength: 128, nullable: true),
                    CreatedDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    WebCreditLogId = table.Column<string>(maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripePaymentIntents", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_StripePaymentIntents_WebCreditLogs",
                table: "StripePaymentIntents",
                column: "WebCreditLogId",
                principalTable: "WebCreditLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StripeCustomers");
        }
    }
}
