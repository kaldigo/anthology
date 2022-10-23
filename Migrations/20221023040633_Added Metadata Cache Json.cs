using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anthology.Migrations
{
    public partial class AddedMetadataCacheJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioBookMetadataJson",
                table: "Books",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BookMetadataJson",
                table: "Books",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioBookMetadataJson",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "BookMetadataJson",
                table: "Books");
        }
    }
}
