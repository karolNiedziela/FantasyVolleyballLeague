using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyVolleyballLeague.VolleyballData.Database.Migrations
{
    /// <inheritdoc />
    public partial class MatchSeason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SeasonId",
                table: "Match",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Existing matches take the season of their round; the foreign key below fails on the empty default.
            migrationBuilder.Sql(
                """
                UPDATE "Match" AS m
                SET "SeasonId" = p."SeasonId"
                FROM "Round" AS r
                JOIN "Phase" AS p ON p."Id" = r."PhaseId"
                WHERE r."Id" = m."RoundId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Match_SeasonId",
                table: "Match",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Season_SeasonId",
                table: "Match",
                column: "SeasonId",
                principalTable: "Season",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Match_Season_SeasonId",
                table: "Match");

            migrationBuilder.DropIndex(
                name: "IX_Match_SeasonId",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Match");
        }
    }
}
