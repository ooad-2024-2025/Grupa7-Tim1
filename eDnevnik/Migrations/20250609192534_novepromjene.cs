using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace eDnevnik.Migrations
{
    /// <inheritdoc />
    public partial class novepromjene : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DanUSedmici",
                table: "Cas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FixniTerminId",
                table: "Cas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FixniTermini",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PocetakVremena = table.Column<TimeSpan>(type: "time", nullable: false),
                    KrajVremena = table.Column<TimeSpan>(type: "time", nullable: false),
                    Redoslijed = table.Column<int>(type: "int", nullable: false),
                    JeOdmor = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixniTermini", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FixniTermini",
                columns: new[] { "Id", "JeOdmor", "KrajVremena", "Naziv", "PocetakVremena", "Redoslijed" },
                values: new object[,]
                {
                    { 1, false, new TimeSpan(0, 8, 45, 0, 0), "1. čas", new TimeSpan(0, 8, 0, 0, 0), 1 },
                    { 2, false, new TimeSpan(0, 9, 35, 0, 0), "2. čas", new TimeSpan(0, 8, 50, 0, 0), 2 },
                    { 3, false, new TimeSpan(0, 10, 25, 0, 0), "3. čas", new TimeSpan(0, 9, 40, 0, 0), 3 },
                    { 4, false, new TimeSpan(0, 11, 30, 0, 0), "4. čas", new TimeSpan(0, 10, 45, 0, 0), 5 },
                    { 5, false, new TimeSpan(0, 12, 20, 0, 0), "5. čas", new TimeSpan(0, 11, 35, 0, 0), 6 },
                    { 6, false, new TimeSpan(0, 13, 10, 0, 0), "6. čas", new TimeSpan(0, 12, 25, 0, 0), 7 },
                    { 7, false, new TimeSpan(0, 14, 15, 0, 0), "7. čas", new TimeSpan(0, 13, 30, 0, 0), 9 },
                    { 8, false, new TimeSpan(0, 15, 5, 0, 0), "8. čas", new TimeSpan(0, 14, 20, 0, 0), 10 },
                    { 9, false, new TimeSpan(0, 15, 55, 0, 0), "9. čas", new TimeSpan(0, 15, 10, 0, 0), 11 },
                    { 100, true, new TimeSpan(0, 10, 45, 0, 0), "Veliki odmor", new TimeSpan(0, 10, 25, 0, 0), 4 },
                    { 101, true, new TimeSpan(0, 13, 30, 0, 0), "Veliki odmor", new TimeSpan(0, 13, 10, 0, 0), 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cas_FixniTerminId",
                table: "Cas",
                column: "FixniTerminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cas_FixniTermini_FixniTerminId",
                table: "Cas",
                column: "FixniTerminId",
                principalTable: "FixniTermini",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cas_FixniTermini_FixniTerminId",
                table: "Cas");

            migrationBuilder.DropTable(
                name: "FixniTermini");

            migrationBuilder.DropIndex(
                name: "IX_Cas_FixniTerminId",
                table: "Cas");

            migrationBuilder.DropColumn(
                name: "DanUSedmici",
                table: "Cas");

            migrationBuilder.DropColumn(
                name: "FixniTerminId",
                table: "Cas");
        }
    }
}
