using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eDnevnik.Migrations
{
    /// <inheritdoc />
    public partial class Aktivnosti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aktivnost",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Prioritet = table.Column<int>(type: "int", nullable: false),
                    NastavnikId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RazredId = table.Column<int>(type: "int", nullable: true),
                    PredmetId = table.Column<int>(type: "int", nullable: true),
                    DatumKreiranja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aktivna = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aktivnost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aktivnost_AspNetUsers_NastavnikId",
                        column: x => x.NastavnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aktivnost_Predmet_PredmetId",
                        column: x => x.PredmetId,
                        principalTable: "Predmet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Aktivnost_Razred_RazredId",
                        column: x => x.RazredId,
                        principalTable: "Razred",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ObavjestenjeLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AktivnostId = table.Column<int>(type: "int", nullable: false),
                    KorisnikId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmailAdresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VrijemeSlanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Greska = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SadržajEmaila = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrojPokušaja = table.Column<int>(type: "int", nullable: false),
                    VrijemeSlijedecegPokušaja = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObavjestenjeLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObavjestenjeLog_Aktivnost_AktivnostId",
                        column: x => x.AktivnostId,
                        principalTable: "Aktivnost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObavjestenjeLog_AspNetUsers_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aktivnost_NastavnikId",
                table: "Aktivnost",
                column: "NastavnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Aktivnost_PredmetId",
                table: "Aktivnost",
                column: "PredmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Aktivnost_RazredId",
                table: "Aktivnost",
                column: "RazredId");

            migrationBuilder.CreateIndex(
                name: "IX_ObavjestenjeLog_AktivnostId",
                table: "ObavjestenjeLog",
                column: "AktivnostId");

            migrationBuilder.CreateIndex(
                name: "IX_ObavjestenjeLog_KorisnikId",
                table: "ObavjestenjeLog",
                column: "KorisnikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObavjestenjeLog");

            migrationBuilder.DropTable(
                name: "Aktivnost");
        }
    }
}
