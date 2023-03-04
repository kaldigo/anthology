using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anthology.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addedprimarybookcoverfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "PersonImage",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "BookImage",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "BookCover",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "AudiobookCover",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "PersonImage");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "BookImage");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "BookCover");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "AudiobookCover");
        }
    }
}
