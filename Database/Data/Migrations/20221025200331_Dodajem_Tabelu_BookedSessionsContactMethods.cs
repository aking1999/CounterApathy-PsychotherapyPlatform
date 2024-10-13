using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_BookedSessionsContactMethods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookedSessionsContactMethods",
                columns: table => new
                {
                    BookedSessionId = table.Column<string>(maxLength: 450, nullable: false),
                    ContactMethodId = table.Column<string>(maxLength: 450, nullable: false),
                    InviteLink = table.Column<string>(maxLength: 1024, nullable: true),
                    LinkAddedDateTime = table.Column<DateTime>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookedSessionsContactMethods", x => new { x.BookedSessionId, x.ContactMethodId });
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BookedSessionsContactMethods_BookedSessions",
                table: "BookedSessionsContactMethods",
                column: "BookedSessionId",
                principalTable: "BookedSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookedSessionsContactMethods_ContactMethods",
                table: "BookedSessionsContactMethods",
                column: "ContactMethodId",
                principalTable: "ContactMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
