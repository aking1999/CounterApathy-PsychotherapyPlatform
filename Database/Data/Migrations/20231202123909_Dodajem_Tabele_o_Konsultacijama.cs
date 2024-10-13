using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Tabele_o_Konsultacijama : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consultations",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    TherapistId = table.Column<string>(maxLength: 450, nullable: false),
                    StartDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    EndDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    Booked = table.Column<int>(maxLength: 16, nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultations_Therapists",
                        column: x => x.TherapistId,
                        principalTable: "Therapists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookedConsultations",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 450, nullable: false),
                    ConsultationId = table.Column<string>(maxLength: 450, nullable: true),
                    TherapistId = table.Column<string>(maxLength: 450, nullable: false),
                    TherapistFirstName = table.Column<string>(maxLength: 128, nullable: false),
                    TherapistLastName = table.Column<string>(maxLength: 128, nullable: false),
                    TherapistEmail = table.Column<string>(maxLength: 256, nullable: false),
                    TherapistPhoneNumber = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(maxLength: 450, nullable: false),
                    ClientFirstName = table.Column<string>(maxLength: 128, nullable: false),
                    ClientLastName = table.Column<string>(maxLength: 128, nullable: false),
                    ClientEmail = table.Column<string>(maxLength: 256, nullable: false),
                    ClientPhoneNumber = table.Column<string>(nullable: true),
                    StartDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    EndDateTime = table.Column<DateTime>(maxLength: 128, nullable: false),
                    BookingDate = table.Column<DateTime>(maxLength: 128, nullable: false),
                    ContactMethodId = table.Column<string>(maxLength: 450, nullable: false),
                    ContactMethodName = table.Column<string>(maxLength: 64, nullable: false),
                    ContactMethodColor = table.Column<string>(maxLength: 64, nullable: false),
                    ContactMethodIcon = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookedConsultations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookedConsultationsContactMethods",
                columns: table => new
                {
                    BookedConsultationId = table.Column<string>(maxLength: 450, nullable: false),
                    ContactMethodId = table.Column<string>(maxLength: 450, nullable: false),
                    InviteLink = table.Column<string>(maxLength: 1024, nullable: true),
                    LinkAddedDateTime = table.Column<DateTime>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookedConsultationsContactMethods", x => new { x.BookedConsultationId, x.ContactMethodId });
                    table.ForeignKey(
                        name: "FK_BookedConsultationsContactMethods_BookedConsultations",
                        column: x => x.BookedConsultationId,
                        principalTable: "BookedConsultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookedConsultationsContactMethods_ContactMethods",
                        column: x => x.ContactMethodId,
                        principalTable: "ContactMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
