using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_Notifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    SenderUserId = table.Column<string>(maxLength: 450, nullable: true),
                    ReceiverUserId = table.Column<string>(maxLength: 450, nullable: false),
                    Title = table.Column<string>(maxLength: 384, nullable: false),
                    Body = table.Column<string>(maxLength: 384, nullable: false),
                    Severity = table.Column<string>(maxLength: 64, nullable: false),
                    Read = table.Column<bool>(maxLength: 128, nullable: false, defaultValue: false),
                    SendingDateTime = table.Column<DateTime>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);

                    table.ForeignKey(
                        name: "FK_NotificationsReceiverUserId_AspNetUsersId",
                        column: x => x.ReceiverUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
