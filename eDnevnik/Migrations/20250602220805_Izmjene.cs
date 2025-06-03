using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eDnevnik.Migrations
{
    /// <inheritdoc />
    public partial class Izmjene : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PredmetRazred_AspNetUsers_NastavnikId",
                table: "PredmetRazred");

            migrationBuilder.DropIndex(
                name: "IX_PredmetRazred_NastavnikId",
                table: "PredmetRazred");

            migrationBuilder.DropColumn(
                name: "NastavnikId",
                table: "PredmetRazred");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NastavnikId",
                table: "PredmetRazred",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PredmetRazred_NastavnikId",
                table: "PredmetRazred",
                column: "NastavnikId");

            migrationBuilder.AddForeignKey(
                name: "FK_PredmetRazred_AspNetUsers_NastavnikId",
                table: "PredmetRazred",
                column: "NastavnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
