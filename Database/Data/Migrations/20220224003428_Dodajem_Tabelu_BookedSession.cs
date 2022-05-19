using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabelu_BookedSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookedSessions",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    SessionId = table.Column<string>(maxLength: 450, nullable: true),
                    TherapistId = table.Column<string>(maxLength: 450, nullable: false),
                    TherapistFirstName = table.Column<string>(maxLength: 128, nullable: false),
                    TherapistLastName = table.Column<string>(maxLength: 128, nullable: false),
                    TherapistEmail = table.Column<string>(maxLength: 256, nullable: false),
                    TherapistPhoneNumber = table.Column<string>(nullable: false),
                    TherapistStreet = table.Column<string>(maxLength: 128, nullable: true, defaultValue: null),
                    TherapistHouseNumber = table.Column<string>(maxLength: 128, nullable: true, defaultValue: null),
                    TherapistCity = table.Column<string>(maxLength: 128, nullable: true, defaultValue: null),
                    TherapistCountry = table.Column<string>(maxLength: 128, nullable: true, defaultValue: null),
                    TherapistPostalCode = table.Column<string>(maxLength: 64, nullable: true, defaultValue: null),
                    ClientId = table.Column<string>(maxLength: 450, nullable: false),
                    ClientFirstName = table.Column<string>(maxLength: 128, nullable: false),
                    ClientLastName = table.Column<string>(maxLength: 128, nullable: false),
                    ClientEmail = table.Column<string>(maxLength: 256, nullable: false),
                    ClientPhoneNumber = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(maxLength: 64, nullable: true),
                    Description = table.Column<string>(maxLength: 192, nullable: true),
                    Price = table.Column<double>(maxLength: 64, nullable: false, defaultValue: 0),
                    Type = table.Column<int>(maxLength: 16, nullable: false, defaultValue: 0),
                    SessionDate = table.Column<DateTime>(maxLength: 128, nullable: false),
                    StartTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    EndTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    BookingDate = table.Column<DateTime>(maxLength: 128, nullable: false),
                    ContactMethodName = table.Column<string>(maxLength: 64, nullable: false),
                    ContactInfo = table.Column<string>(maxLength: 450, nullable: true),
                    Status = table.Column<int>(maxLength: 16, nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookedSessions", x => x.Id);
                });

            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
