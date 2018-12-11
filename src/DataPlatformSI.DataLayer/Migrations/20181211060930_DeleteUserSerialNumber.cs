using Microsoft.EntityFrameworkCore.Migrations;

namespace DataPlatformSI.DataLayer.Migrations
{
    public partial class DeleteUserSerialNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "AppUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "AppUsers",
                nullable: true);
        }
    }
}
