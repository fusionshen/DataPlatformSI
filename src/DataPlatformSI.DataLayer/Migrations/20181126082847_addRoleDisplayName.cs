using Microsoft.EntityFrameworkCore.Migrations;

namespace DataPlatformSI.DataLayer.Migrations
{
    public partial class addRoleDisplayName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AppRoles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AppRoles");
        }
    }
}
