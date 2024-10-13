using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Kolone_u_WebCreditLogs_i_Tabelu_PaymentTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: true),
                    Logo = table.Column<string>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                });

            migrationBuilder.AddColumn<string>(
                name: "PaymentTypeId",
                table: "WebCreditLogs",
                maxLength: 450,
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTypeName",
                table: "WebCreditLogs",
                maxLength: 128,
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddForeignKey(
                name: "FK_WebCreditLogs_PaymentTypes",
                table: "WebCreditLogs",
                column: "PaymentTypeId",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
