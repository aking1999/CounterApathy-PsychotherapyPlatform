using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_ClientSupportTickets_i_TherapistSupportTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientSupportTicketTopics",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: true),
                    Color = table.Column<string>(maxLength: 32, nullable: true),
                    Icon = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSupportTicketTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TherapistSupportTicketTopics",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: true),
                    Color = table.Column<string>(maxLength: 32, nullable: true),
                    Icon = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapistSupportTicketTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientSupportTickets",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    UserIdOrAnonymous = table.Column<string>(maxLength: 450, nullable: false),
                    FirstName = table.Column<string>(maxLength: 128, nullable: false),
                    LastName = table.Column<string>(maxLength: 128, nullable: false),
                    Email = table.Column<string>(maxLength: 256, nullable: false),
                    Text = table.Column<string>(maxLength: 2048, nullable: false),
                    TopicId = table.Column<string>(maxLength: 450, nullable: true),
                    TopicName = table.Column<string>(maxLength: 256, nullable: false),
                    TicketDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    AdminIdWhoAnswered = table.Column<string>(maxLength: 450, nullable: true),
                    Answered = table.Column<bool>(maxLength: 128, nullable: false, defaultValue: false),
                    AnsweredDateTime = table.Column<DateTime>(maxLength: 128, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSupportTickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TherapistSupportTickets",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    TherapistId = table.Column<string>(maxLength: 450, nullable: true),
                    FirstName = table.Column<string>(maxLength: 128, nullable: false),
                    LastName = table.Column<string>(maxLength: 128, nullable: false),
                    Email = table.Column<string>(maxLength: 256, nullable: false),
                    Text = table.Column<string>(maxLength: 2048, nullable: false),
                    TopicId = table.Column<string>(maxLength: 450, nullable: true),
                    TopicName = table.Column<string>(maxLength: 256, nullable: false),
                    TicketDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    AdminIdWhoAnswered = table.Column<string>(maxLength: 450, nullable: true),
                    Answered = table.Column<bool>(maxLength: 128, nullable: false, defaultValue: false),
                    AnsweredDateTime = table.Column<DateTime>(maxLength: 128, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapistSupportTickets", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ClientSupportTickets_ClientSupportTicketTopics",
                table: "ClientSupportTickets",
                column: "TopicId",
                principalTable: "ClientSupportTicketTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapistSupportTickets_TherapistSupportTicketTopics",
                table: "TherapistSupportTickets",
                column: "TopicId",
                principalTable: "TherapistSupportTicketTopics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
