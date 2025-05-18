using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eDnevnik.Migrations
{
    /// <inheritdoc />
    public partial class Migracija2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UcenikPredmet_AspNetUsers_UcenikId1",
                table: "UcenikPredmet");

            migrationBuilder.DropForeignKey(
                name: "FK_UcenikPredmet_Predmet_PredmetId",
                table: "UcenikPredmet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UcenikPredmet",
                table: "UcenikPredmet");

            migrationBuilder.RenameTable(
                name: "UcenikPredmet",
                newName: "RazredPredmet");

            migrationBuilder.RenameIndex(
                name: "IX_UcenikPredmet_UcenikId1",
                table: "RazredPredmet",
                newName: "IX_RazredPredmet_UcenikId1");

            migrationBuilder.RenameIndex(
                name: "IX_UcenikPredmet_PredmetId",
                table: "RazredPredmet",
                newName: "IX_RazredPredmet_PredmetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RazredPredmet",
                table: "RazredPredmet",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RazredPredmet_AspNetUsers_UcenikId1",
                table: "RazredPredmet",
                column: "UcenikId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RazredPredmet_Predmet_PredmetId",
                table: "RazredPredmet",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RazredPredmet_AspNetUsers_UcenikId1",
                table: "RazredPredmet");

            migrationBuilder.DropForeignKey(
                name: "FK_RazredPredmet_Predmet_PredmetId",
                table: "RazredPredmet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RazredPredmet",
                table: "RazredPredmet");

            migrationBuilder.RenameTable(
                name: "RazredPredmet",
                newName: "UcenikPredmet");

            migrationBuilder.RenameIndex(
                name: "IX_RazredPredmet_UcenikId1",
                table: "UcenikPredmet",
                newName: "IX_UcenikPredmet_UcenikId1");

            migrationBuilder.RenameIndex(
                name: "IX_RazredPredmet_PredmetId",
                table: "UcenikPredmet",
                newName: "IX_UcenikPredmet_PredmetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UcenikPredmet",
                table: "UcenikPredmet",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UcenikPredmet_AspNetUsers_UcenikId1",
                table: "UcenikPredmet",
                column: "UcenikId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UcenikPredmet_Predmet_PredmetId",
                table: "UcenikPredmet",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
