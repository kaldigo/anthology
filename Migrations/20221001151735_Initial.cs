using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anthology.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookAuthor",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAuthor", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BookFunnelItems",
                columns: table => new
                {
                    ID = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Downloaded = table.Column<bool>(type: "INTEGER", nullable: false),
                    Extracted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ZipPath = table.Column<string>(type: "TEXT", nullable: true),
                    ExtractedPath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookFunnelItems", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BookGenre",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookGenre", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BookNarrator",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookNarrator", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    ISBN = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Subtitle = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", nullable: true),
                    PublishDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    IsExplicit = table.Column<bool>(type: "INTEGER", nullable: false),
                    GRID = table.Column<string>(type: "TEXT", nullable: false),
                    ASIN = table.Column<string>(type: "TEXT", nullable: true),
                    AudibleExists = table.Column<bool>(type: "INTEGER", nullable: false),
                    AGID = table.Column<string>(type: "TEXT", nullable: true),
                    AudiobookGuildExists = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.ISBN);
                });

            migrationBuilder.CreateTable(
                name: "BookSeries",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Sequence = table.Column<float>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookSeries", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BookTag",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTag", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Classifications",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classifications", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AudiobookCover",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudiobookCover", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AudiobookCover_Books_BookISBN",
                        column: x => x.BookISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN");
                });

            migrationBuilder.CreateTable(
                name: "BookBookAuthor",
                columns: table => new
                {
                    AuthorsID = table.Column<Guid>(type: "TEXT", nullable: false),
                    BooksISBN = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookBookAuthor", x => new { x.AuthorsID, x.BooksISBN });
                    table.ForeignKey(
                        name: "FK_BookBookAuthor_BookAuthor_AuthorsID",
                        column: x => x.AuthorsID,
                        principalTable: "BookAuthor",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookBookAuthor_Books_BooksISBN",
                        column: x => x.BooksISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookBookGenre",
                columns: table => new
                {
                    BooksISBN = table.Column<string>(type: "TEXT", nullable: false),
                    GenresID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookBookGenre", x => new { x.BooksISBN, x.GenresID });
                    table.ForeignKey(
                        name: "FK_BookBookGenre_BookGenre_GenresID",
                        column: x => x.GenresID,
                        principalTable: "BookGenre",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookBookGenre_Books_BooksISBN",
                        column: x => x.BooksISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookBookNarrator",
                columns: table => new
                {
                    BooksISBN = table.Column<string>(type: "TEXT", nullable: false),
                    NarratorsID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookBookNarrator", x => new { x.BooksISBN, x.NarratorsID });
                    table.ForeignKey(
                        name: "FK_BookBookNarrator_BookNarrator_NarratorsID",
                        column: x => x.NarratorsID,
                        principalTable: "BookNarrator",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookBookNarrator_Books_BooksISBN",
                        column: x => x.BooksISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCover",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCover", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BookCover_Books_BookISBN",
                        column: x => x.BookISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN");
                });

            migrationBuilder.CreateTable(
                name: "BookImage",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookImage", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BookImage_Books_BookISBN",
                        column: x => x.BookISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN");
                });

            migrationBuilder.CreateTable(
                name: "BookBookSeries",
                columns: table => new
                {
                    BooksISBN = table.Column<string>(type: "TEXT", nullable: false),
                    SeriesID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookBookSeries", x => new { x.BooksISBN, x.SeriesID });
                    table.ForeignKey(
                        name: "FK_BookBookSeries_Books_BooksISBN",
                        column: x => x.BooksISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookBookSeries_BookSeries_SeriesID",
                        column: x => x.SeriesID,
                        principalTable: "BookSeries",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookBookTag",
                columns: table => new
                {
                    BooksISBN = table.Column<string>(type: "TEXT", nullable: false),
                    TagsID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookBookTag", x => new { x.BooksISBN, x.TagsID });
                    table.ForeignKey(
                        name: "FK_BookBookTag_Books_BooksISBN",
                        column: x => x.BooksISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookBookTag_BookTag_TagsID",
                        column: x => x.TagsID,
                        principalTable: "BookTag",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassificationAlias",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ClassificationID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificationAlias", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ClassificationAlias_Classifications_ClassificationID",
                        column: x => x.ClassificationID,
                        principalTable: "Classifications",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AudiobookCover_BookISBN",
                table: "AudiobookCover",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_BookBookAuthor_BooksISBN",
                table: "BookBookAuthor",
                column: "BooksISBN");

            migrationBuilder.CreateIndex(
                name: "IX_BookBookGenre_GenresID",
                table: "BookBookGenre",
                column: "GenresID");

            migrationBuilder.CreateIndex(
                name: "IX_BookBookNarrator_NarratorsID",
                table: "BookBookNarrator",
                column: "NarratorsID");

            migrationBuilder.CreateIndex(
                name: "IX_BookBookSeries_SeriesID",
                table: "BookBookSeries",
                column: "SeriesID");

            migrationBuilder.CreateIndex(
                name: "IX_BookBookTag_TagsID",
                table: "BookBookTag",
                column: "TagsID");

            migrationBuilder.CreateIndex(
                name: "IX_BookCover_BookISBN",
                table: "BookCover",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_BookImage_BookISBN",
                table: "BookImage",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_ClassificationAlias_ClassificationID",
                table: "ClassificationAlias",
                column: "ClassificationID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudiobookCover");

            migrationBuilder.DropTable(
                name: "BookBookAuthor");

            migrationBuilder.DropTable(
                name: "BookBookGenre");

            migrationBuilder.DropTable(
                name: "BookBookNarrator");

            migrationBuilder.DropTable(
                name: "BookBookSeries");

            migrationBuilder.DropTable(
                name: "BookBookTag");

            migrationBuilder.DropTable(
                name: "BookCover");

            migrationBuilder.DropTable(
                name: "BookFunnelItems");

            migrationBuilder.DropTable(
                name: "BookImage");

            migrationBuilder.DropTable(
                name: "ClassificationAlias");

            migrationBuilder.DropTable(
                name: "BookAuthor");

            migrationBuilder.DropTable(
                name: "BookGenre");

            migrationBuilder.DropTable(
                name: "BookNarrator");

            migrationBuilder.DropTable(
                name: "BookSeries");

            migrationBuilder.DropTable(
                name: "BookTag");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Classifications");
        }
    }
}
