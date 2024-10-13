using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class Dodajem_Kolone_za_Dijagnostiku_u_Tabelu_ErrorLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StackTraceFrameMethodName",
                table: "ErrorLogs",
                nullable: true,
                maxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "StackTraceExecutingAssemblyName",
                table: "ErrorLogs",
                nullable: true,
                maxLength: 512);

             migrationBuilder.AddColumn<string>(
                name: "TargetSiteName",
                table: "ErrorLogs",
                nullable: true,
                maxLength: 512);

             migrationBuilder.AddColumn<string>(
                name: "TargetSiteReflectedTypeFullName",
                table: "ErrorLogs",
                nullable: true,
                maxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "StackTrace",
                table: "ErrorLogs",
                nullable: true,
                maxLength: 2048);

            migrationBuilder.AddColumn<bool>(
                name: "Fixed",
                table: "ErrorLogs",
                nullable: false,
                maxLength: 128,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
