using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_Transactions_i_TransactionId_u_Tabelu_BookedSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "BookedSessions",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    SenderId = table.Column<string>(maxLength: 450, nullable: false),
                    ReceiverId = table.Column<string>(maxLength: 450, nullable: false),
                    DateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    Amount = table.Column<double>(maxLength: 128, nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
