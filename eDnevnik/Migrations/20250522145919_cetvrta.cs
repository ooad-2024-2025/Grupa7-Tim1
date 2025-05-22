using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eDnevnik.Migrations
{
    /// <inheritdoc />
    public partial class cetvrta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RazredPredmet_AspNetUsers_UcenikId",
                table: "RazredPredmet");

            migrationBuilder.DropForeignKey(
                name: "FK_RazredPredmet_Predmet_PredmetId",
                table: "RazredPredmet");

            migrationBuilder.AddForeignKey(
                name: "FK_RazredPredmet_AspNetUsers_UcenikId",
                table: "RazredPredmet",
                column: "UcenikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_RazredPredmet_AspNetUsers_UcenikId",
                table: "RazredPredmet");

            migrationBuilder.DropForeignKey(
                name: "FK_RazredPredmet_Predmet_PredmetId",
                table: "RazredPredmet");

            migrationBuilder.AddForeignKey(
                name: "FK_RazredPredmet_AspNetUsers_UcenikId",
                table: "RazredPredmet",
                column: "UcenikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RazredPredmet_Predmet_PredmetId",
                table: "RazredPredmet",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
