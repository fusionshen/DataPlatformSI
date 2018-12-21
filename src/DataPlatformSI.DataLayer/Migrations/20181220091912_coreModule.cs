using Microsoft.EntityFrameworkCore.Migrations;

namespace DataPlatformSI.DataLayer.Migrations
{
    public partial class coreModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCustom",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "ModuleType",
                table: "Modules");

            migrationBuilder.AddColumn<bool>(
                name: "IsCore",
                table: "Modules",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCore",
                table: "Modules");

            migrationBuilder.AddColumn<bool>(
                name: "IsCustom",
                table: "Modules",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModuleType",
                table: "Modules",
                nullable: true);
        }
    }
}
