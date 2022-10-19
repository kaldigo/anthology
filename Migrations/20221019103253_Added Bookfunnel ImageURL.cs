using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anthology.Migrations
{
    public partial class AddedBookfunnelImageURL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "BookFunnelItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "BookFunnelItems");
        }
    }
}
