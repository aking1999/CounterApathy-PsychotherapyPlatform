using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_Withdrawals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Withdrawals",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    TherapistId = table.Column<string>(maxLength: 450, nullable: true),
                    FirstName = table.Column<string>(maxLength: 128, nullable: false),
                    LastName = table.Column<string>(maxLength: 128, nullable: false),
                    Street = table.Column<string>(maxLength: 128, nullable: false),
                    HouseNumber = table.Column<string>(maxLength: 128, nullable: false),
                    City = table.Column<string>(maxLength: 128, nullable: false),
                    PostalCode = table.Column<string>(maxLength: 64, nullable: false),
                    Country = table.Column<string>(maxLength: 128, nullable: false),
                    Email = table.Column<string>(maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(nullable: false),
                    Status = table.Column<int>(maxLength: 4, nullable: false, defaultValue: 0),
                    BankAccountNumber = table.Column<string>(maxLength: 128, nullable: false),
                    Amount = table.Column<double>(maxLength: 64, nullable: false),
                    RequestDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    AcceptDateTime = table.Column<DateTime>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Withdrawals", x => x.Id);

                    table.ForeignKey(
                        name: "FK_WithdrawalsTherapistId_TherapistsId",
                        column: x => x.TherapistId,
                        principalTable: "Therapists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
