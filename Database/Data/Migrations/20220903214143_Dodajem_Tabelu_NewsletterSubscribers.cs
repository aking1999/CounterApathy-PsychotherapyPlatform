using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_NewsletterSubscribers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsletterSubscribers",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    Email = table.Column<string>(maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: false),
                    NotifiedCount = table.Column<int>(maxLength: 32, nullable: false, defaultValue: 0),
                    IpAddress = table.Column<string>(maxLength: 256, nullable: true),
                    LastNotifiedDateTime = table.Column<DateTime>(maxLength: 128, nullable: true),
                    SubscribeDateTime = table.Column<DateTime>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscribers", x => x.Id);
                });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
