using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eDnevnik.Migrations
{
    /// <inheritdoc />
    public partial class nova3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cas_Predmet_PredmetId",
                table: "Cas");

            migrationBuilder.DropForeignKey(
                name: "FK_Cas_Razred_RazredId",
                table: "Cas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Predmet_PredmetId",
                table: "Ocjena");

            migrationBuilder.AddForeignKey(
                name: "FK_Cas_Predmet_PredmetId",
                table: "Cas",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cas_Razred_RazredId",
                table: "Cas",
                column: "RazredId",
                principalTable: "Razred",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Predmet_PredmetId",
                table: "Ocjena",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cas_Predmet_PredmetId",
                table: "Cas");

            migrationBuilder.DropForeignKey(
                name: "FK_Cas_Razred_RazredId",
                table: "Cas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Predmet_PredmetId",
                table: "Ocjena");

            migrationBuilder.AddForeignKey(
                name: "FK_Cas_Predmet_PredmetId",
                table: "Cas",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cas_Razred_RazredId",
                table: "Cas",
                column: "RazredId",
                principalTable: "Razred",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Predmet_PredmetId",
                table: "Ocjena",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
