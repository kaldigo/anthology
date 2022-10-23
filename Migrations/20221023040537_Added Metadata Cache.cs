using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anthology.Migrations
{
    public partial class AddedMetadataCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateFetchedMetadata",
                table: "Books",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SubtitleLock",
                table: "Books",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFetchedMetadata",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "SubtitleLock",
                table: "Books");
        }
    }
}
