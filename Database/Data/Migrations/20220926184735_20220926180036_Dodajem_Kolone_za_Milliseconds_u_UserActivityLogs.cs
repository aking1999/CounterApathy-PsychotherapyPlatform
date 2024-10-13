using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Data.Migrations
{
    public partial class _20220926180036_Dodajem_Kolone_za_Milliseconds_u_UserActivityLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ActionExecutedMilliseconds",
                table: "UserActivityLogs",
                nullable: false,
                maxLength: 128,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ResultExecutedMilliseconds",
                table: "UserActivityLogs",
                nullable: false,
                maxLength: 128,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
