using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eDnevnik.Migrations
{
    /// <inheritdoc />
    public partial class EvidencijaCasa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvidencijaCasa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CasId = table.Column<int>(type: "int", nullable: false),
                    DatumOdrzavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aktivnosti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Napomene = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Odrzan = table.Column<bool>(type: "bit", nullable: false),
                    NastavnikId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VrijemeEvidentiranja = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidencijaCasa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidencijaCasa_AspNetUsers_NastavnikId",
                        column: x => x.NastavnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvidencijaCasa_Cas_CasId",
                        column: x => x.CasId,
                        principalTable: "Cas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidencijaCasa_CasId",
                table: "EvidencijaCasa",
                column: "CasId");

            migrationBuilder.CreateIndex(
                name: "IX_EvidencijaCasa_NastavnikId",
                table: "EvidencijaCasa",
                column: "NastavnikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidencijaCasa");
        }
    }
}
