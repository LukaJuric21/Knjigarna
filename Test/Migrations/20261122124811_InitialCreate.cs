using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Knjige",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Naslov = table.Column<string>(type: "TEXT", nullable: false),
                    Avtor = table.Column<string>(type: "TEXT", nullable: false),
                    Opis = table.Column<string>(type: "TEXT", nullable: false),
                    Cena = table.Column<decimal>(type: "TEXT", nullable: false),
                    DatumDodajanja = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Knjige", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zanri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Naziv = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zanri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnjigaZanr",
                columns: table => new
                {
                    KnjigeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ZanriId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnjigaZanr", x => new { x.KnjigeId, x.ZanriId });
                    table.ForeignKey(
                        name: "FK_KnjigaZanr_Knjige_KnjigeId",
                        column: x => x.KnjigeId,
                        principalTable: "Knjige",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KnjigaZanr_Zanri_ZanriId",
                        column: x => x.ZanriId,
                        principalTable: "Zanri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnjigaZanr_ZanriId",
                table: "KnjigaZanr",
                column: "ZanriId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnjigaZanr");

            migrationBuilder.DropTable(
                name: "Knjige");

            migrationBuilder.DropTable(
                name: "Zanri");
        }
    }
}
