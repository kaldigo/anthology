using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Anthology.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    ISBN = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Subtitle = table.Column<string>(type: "TEXT", nullable: true),
                    SubtitleLock = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", nullable: true),
                    PublishDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    IsExplicit = table.Column<bool>(type: "INTEGER", nullable: false),
                    BookMetadataJson = table.Column<string>(type: "TEXT", nullable: true),
                    DateFetchedMetadata = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.ISBN);
                });

            migrationBuilder.CreateTable(
                name: "FieldPriorities",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    MergeGenres = table.Column<bool>(type: "INTEGER", nullable: false),
                    MergeTags = table.Column<bool>(type: "INTEGER", nullable: false),
                    MergeCovers = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldPriorities", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AudiobookCover",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "BookCover",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "BookIdentifier",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookIdentifier", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BookIdentifier_Books_BookISBN",
                        column: x => x.BookISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN");
                });

            migrationBuilder.CreateTable(
                name: "BookImage",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "Downloads",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    ImageURL = table.Column<string>(type: "TEXT", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "Settings",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldPrioritiesID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Settings_FieldPriorities_FieldPrioritiesID",
                        column: x => x.FieldPrioritiesID,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SourcePriority",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FieldPrioritiesID = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID1 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID10 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID11 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID12 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID2 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID3 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID4 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID5 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID6 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID7 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID8 = table.Column<Guid>(type: "TEXT", nullable: true),
                    FieldPrioritiesID9 = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourcePriority", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID",
                        column: x => x.FieldPrioritiesID,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID1",
                        column: x => x.FieldPrioritiesID1,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID10",
                        column: x => x.FieldPrioritiesID10,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID11",
                        column: x => x.FieldPrioritiesID11,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID12",
                        column: x => x.FieldPrioritiesID12,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID2",
                        column: x => x.FieldPrioritiesID2,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID3",
                        column: x => x.FieldPrioritiesID3,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID4",
                        column: x => x.FieldPrioritiesID4,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID5",
                        column: x => x.FieldPrioritiesID5,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID6",
                        column: x => x.FieldPrioritiesID6,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID7",
                        column: x => x.FieldPrioritiesID7,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID8",
                        column: x => x.FieldPrioritiesID8,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SourcePriority_FieldPriorities_FieldPrioritiesID9",
                        column: x => x.FieldPrioritiesID9,
                        principalTable: "FieldPriorities",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "BookPerson",
                columns: table => new
                {
                    AuthorsID = table.Column<Guid>(type: "TEXT", nullable: false),
                    BooksAuthoredISBN = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookPerson", x => new { x.AuthorsID, x.BooksAuthoredISBN });
                    table.ForeignKey(
                        name: "FK_BookPerson_Books_BooksAuthoredISBN",
                        column: x => x.BooksAuthoredISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookPerson_People_AuthorsID",
                        column: x => x.AuthorsID,
                        principalTable: "People",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookPerson1",
                columns: table => new
                {
                    BooksNarratedISBN = table.Column<string>(type: "TEXT", nullable: false),
                    NarratorsID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookPerson1", x => new { x.BooksNarratedISBN, x.NarratorsID });
                    table.ForeignKey(
                        name: "FK_BookPerson1_Books_BooksNarratedISBN",
                        column: x => x.BooksNarratedISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookPerson1_People_NarratorsID",
                        column: x => x.NarratorsID,
                        principalTable: "People",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonAlias",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PersonID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonAlias", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PersonAlias_People_PersonID",
                        column: x => x.PersonID,
                        principalTable: "People",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PersonImage",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PersonID = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonImage", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PersonImage_People_PersonID",
                        column: x => x.PersonID,
                        principalTable: "People",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookSeries",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Sequence = table.Column<string>(type: "TEXT", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true),
                    SeriesID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookSeries", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BookSeries_Books_BookISBN",
                        column: x => x.BookISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN");
                    table.ForeignKey(
                        name: "FK_BookSeries_Series_SeriesID",
                        column: x => x.SeriesID,
                        principalTable: "Series",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Classifications",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    BookISBN = table.Column<string>(type: "TEXT", nullable: true),
                    SeriesID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classifications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Classifications_Books_BookISBN",
                        column: x => x.BookISBN,
                        principalTable: "Books",
                        principalColumn: "ISBN");
                    table.ForeignKey(
                        name: "FK_Classifications_Series_SeriesID",
                        column: x => x.SeriesID,
                        principalTable: "Series",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "SeriesAlias",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SeriesID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesAlias", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SeriesAlias_Series_SeriesID",
                        column: x => x.SeriesID,
                        principalTable: "Series",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DownloadAuthor",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadID = table.Column<Guid>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "PluginSetting",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PluginName = table.Column<string>(type: "TEXT", nullable: false),
                    SettingsID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginSetting", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PluginSetting_Settings_SettingsID",
                        column: x => x.SettingsID,
                        principalTable: "Settings",
                        principalColumn: "ID");
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

            migrationBuilder.CreateTable(
                name: "SettingKV",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    PluginSettingID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingKV", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SettingKV_PluginSetting_PluginSettingID",
                        column: x => x.PluginSettingID,
                        principalTable: "PluginSetting",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AudiobookCover_BookISBN",
                table: "AudiobookCover",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_BookCover_BookISBN",
                table: "BookCover",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_BookIdentifier_BookISBN",
                table: "BookIdentifier",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_BookImage_BookISBN",
                table: "BookImage",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_BookPerson_BooksAuthoredISBN",
                table: "BookPerson",
                column: "BooksAuthoredISBN");

            migrationBuilder.CreateIndex(
                name: "IX_BookPerson1_NarratorsID",
                table: "BookPerson1",
                column: "NarratorsID");

            migrationBuilder.CreateIndex(
                name: "IX_BookSeries_BookISBN",
                table: "BookSeries",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_BookSeries_SeriesID",
                table: "BookSeries",
                column: "SeriesID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassificationAlias_ClassificationID",
                table: "ClassificationAlias",
                column: "ClassificationID");

            migrationBuilder.CreateIndex(
                name: "IX_Classifications_BookISBN",
                table: "Classifications",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_Classifications_SeriesID",
                table: "Classifications",
                column: "SeriesID");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadAuthor_DownloadID",
                table: "DownloadAuthor",
                column: "DownloadID");

            migrationBuilder.CreateIndex(
                name: "IX_Downloads_BookISBN",
                table: "Downloads",
                column: "BookISBN");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAlias_PersonID",
                table: "PersonAlias",
                column: "PersonID");

            migrationBuilder.CreateIndex(
                name: "IX_PersonImage_PersonID",
                table: "PersonImage",
                column: "PersonID");

            migrationBuilder.CreateIndex(
                name: "IX_PluginSetting_SettingsID",
                table: "PluginSetting",
                column: "SettingsID");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesAlias_SeriesID",
                table: "SeriesAlias",
                column: "SeriesID");

            migrationBuilder.CreateIndex(
                name: "IX_SettingKV_PluginSettingID",
                table: "SettingKV",
                column: "PluginSettingID");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_FieldPrioritiesID",
                table: "Settings",
                column: "FieldPrioritiesID");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID",
                table: "SourcePriority",
                column: "FieldPrioritiesID");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID1",
                table: "SourcePriority",
                column: "FieldPrioritiesID1");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID10",
                table: "SourcePriority",
                column: "FieldPrioritiesID10");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID11",
                table: "SourcePriority",
                column: "FieldPrioritiesID11");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID12",
                table: "SourcePriority",
                column: "FieldPrioritiesID12");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID2",
                table: "SourcePriority",
                column: "FieldPrioritiesID2");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID3",
                table: "SourcePriority",
                column: "FieldPrioritiesID3");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID4",
                table: "SourcePriority",
                column: "FieldPrioritiesID4");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID5",
                table: "SourcePriority",
                column: "FieldPrioritiesID5");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID6",
                table: "SourcePriority",
                column: "FieldPrioritiesID6");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID7",
                table: "SourcePriority",
                column: "FieldPrioritiesID7");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID8",
                table: "SourcePriority",
                column: "FieldPrioritiesID8");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePriority_FieldPrioritiesID9",
                table: "SourcePriority",
                column: "FieldPrioritiesID9");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudiobookCover");

            migrationBuilder.DropTable(
                name: "BookCover");

            migrationBuilder.DropTable(
                name: "BookIdentifier");

            migrationBuilder.DropTable(
                name: "BookImage");

            migrationBuilder.DropTable(
                name: "BookPerson");

            migrationBuilder.DropTable(
                name: "BookPerson1");

            migrationBuilder.DropTable(
                name: "BookSeries");

            migrationBuilder.DropTable(
                name: "ClassificationAlias");

            migrationBuilder.DropTable(
                name: "DownloadAuthor");

            migrationBuilder.DropTable(
                name: "PersonAlias");

            migrationBuilder.DropTable(
                name: "PersonImage");

            migrationBuilder.DropTable(
                name: "SeriesAlias");

            migrationBuilder.DropTable(
                name: "SettingKV");

            migrationBuilder.DropTable(
                name: "SourcePriority");

            migrationBuilder.DropTable(
                name: "Classifications");

            migrationBuilder.DropTable(
                name: "Downloads");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "PluginSetting");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "FieldPriorities");
        }
    }
}
