using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class PlayerGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Player_ExternalId",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "GenderId",
                table: "Player",
                type: "integer",
                nullable: false,
                defaultValue: 1); // Existing players were all imported from the men's leagues (LeagueGender.Men).

            migrationBuilder.CreateIndex(
                name: "IX_Player_GenderId_ExternalId",
                table: "Player",
                columns: new[] { "GenderId", "ExternalId" },
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Player_LeagueGender_GenderId",
                table: "Player",
                column: "GenderId",
                principalTable: "LeagueGender",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Player_LeagueGender_GenderId",
                table: "Player");

            migrationBuilder.DropIndex(
                name: "IX_Player_GenderId_ExternalId",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "Player");

            migrationBuilder.CreateIndex(
                name: "IX_Player_ExternalId",
                table: "Player",
                column: "ExternalId",
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");
        }
    }
}
