using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataPlatformSI.DataLayer.Migrations
{
    public partial class V201811061732 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AccessTokenExpiresDateTime",
                table: "AppUserTokens",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "AccessTokenHash",
                table: "AppUserTokens",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RefreshTokenExpiresDateTime",
                table: "AppUserTokens",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenIdHash",
                table: "AppUserTokens",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenIdHashSource",
                table: "AppUserTokens",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "AppUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessTokenExpiresDateTime",
                table: "AppUserTokens");

            migrationBuilder.DropColumn(
                name: "AccessTokenHash",
                table: "AppUserTokens");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiresDateTime",
                table: "AppUserTokens");

            migrationBuilder.DropColumn(
                name: "RefreshTokenIdHash",
                table: "AppUserTokens");

            migrationBuilder.DropColumn(
                name: "RefreshTokenIdHashSource",
                table: "AppUserTokens");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "AppUsers");
        }
    }
}
