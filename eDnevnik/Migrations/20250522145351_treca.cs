using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eDnevnik.Migrations
{
    /// <inheritdoc />
    public partial class treca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_RoditeljId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Razred_RazredId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Cas_Predmet_PredmetId",
                table: "Cas");

            migrationBuilder.DropForeignKey(
                name: "FK_Cas_Razred_RazredId",
                table: "Cas");

            migrationBuilder.DropForeignKey(
                name: "FK_Izostanak_AspNetUsers_UcenikId",
                table: "Izostanak");

            migrationBuilder.DropForeignKey(
                name: "FK_Izostanak_Cas_CasId",
                table: "Izostanak");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_AspNetUsers_UcenikId",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Predmet_PredmetId",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_AspNetUsers_NastavnikId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_RazredPredmet_AspNetUsers_UcenikId",
                table: "RazredPredmet");

            migrationBuilder.DropForeignKey(
                name: "FK_RazredPredmet_Predmet_PredmetId",
                table: "RazredPredmet");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RazredId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "NastavnikId",
                table: "Razred",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Razred_NastavnikId",
                table: "Razred",
                column: "NastavnikId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RazredId",
                table: "AspNetUsers",
                column: "RazredId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_RoditeljId",
                table: "AspNetUsers",
                column: "RoditeljId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Razred_RazredId",
                table: "AspNetUsers",
                column: "RazredId",
                principalTable: "Razred",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Izostanak_AspNetUsers_UcenikId",
                table: "Izostanak",
                column: "UcenikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Izostanak_Cas_CasId",
                table: "Izostanak",
                column: "CasId",
                principalTable: "Cas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_AspNetUsers_UcenikId",
                table: "Ocjena",
                column: "UcenikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Predmet_PredmetId",
                table: "Ocjena",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_AspNetUsers_NastavnikId",
                table: "Predmet",
                column: "NastavnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Razred_AspNetUsers_NastavnikId",
                table: "Razred",
                column: "NastavnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_RoditeljId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Razred_RazredId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Cas_Predmet_PredmetId",
                table: "Cas");

            migrationBuilder.DropForeignKey(
                name: "FK_Cas_Razred_RazredId",
                table: "Cas");

            migrationBuilder.DropForeignKey(
                name: "FK_Izostanak_AspNetUsers_UcenikId",
                table: "Izostanak");

            migrationBuilder.DropForeignKey(
                name: "FK_Izostanak_Cas_CasId",
                table: "Izostanak");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_AspNetUsers_UcenikId",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Ocjena_Predmet_PredmetId",
                table: "Ocjena");

            migrationBuilder.DropForeignKey(
                name: "FK_Predmet_AspNetUsers_NastavnikId",
                table: "Predmet");

            migrationBuilder.DropForeignKey(
                name: "FK_Razred_AspNetUsers_NastavnikId",
                table: "Razred");

            migrationBuilder.DropForeignKey(
                name: "FK_RazredPredmet_AspNetUsers_UcenikId",
                table: "RazredPredmet");

            migrationBuilder.DropForeignKey(
                name: "FK_RazredPredmet_Predmet_PredmetId",
                table: "RazredPredmet");

            migrationBuilder.DropIndex(
                name: "IX_Razred_NastavnikId",
                table: "Razred");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RazredId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "NastavnikId",
                table: "Razred",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RazredId",
                table: "AspNetUsers",
                column: "RazredId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_RoditeljId",
                table: "AspNetUsers",
                column: "RoditeljId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Razred_RazredId",
                table: "AspNetUsers",
                column: "RazredId",
                principalTable: "Razred",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Izostanak_AspNetUsers_UcenikId",
                table: "Izostanak",
                column: "UcenikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Izostanak_Cas_CasId",
                table: "Izostanak",
                column: "CasId",
                principalTable: "Cas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_AspNetUsers_UcenikId",
                table: "Ocjena",
                column: "UcenikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ocjena_Predmet_PredmetId",
                table: "Ocjena",
                column: "PredmetId",
                principalTable: "Predmet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Predmet_AspNetUsers_NastavnikId",
                table: "Predmet",
                column: "NastavnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
