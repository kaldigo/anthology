using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anthology.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedDownloadsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadAuthor");

            migrationBuilder.DropTable(
                name: "TempImages");

            migrationBuilder.DropTable(
                name: "Downloads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Downloads",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    ImageURL = table.Column<string>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downloads", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Downloads_Books_BookISBN",
                        column: x => x.BookISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN");
                });

            migrationBuilder.CreateTable(
                name: "TempImages",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    TempPath = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempImages", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DownloadAuthor",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DownloadID = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadAuthor", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DownloadAuthor_Downloads_DownloadID",
                        column: x => x.DownloadID,
                        principalTable: "Downloads",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadAuthor_DownloadID",
                table: "DownloadAuthor",
                column: "DownloadID");

            migrationBuilder.CreateIndex(
                name: "IX_Downloads_BookISBN",
                table: "Downloads",
                column: "BookISBN");
        }
    }
}
