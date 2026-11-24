using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyVolleyballLeague.VolleyballData.Database.Migrations
{
    /// <inheritdoc />
    public partial class Clubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClubId",
                table: "Roster",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Club",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "pl-x-icu"),
                    NormalizedName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GenderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Club", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Club_LeagueGender_GenderId",
                        column: x => x.GenderId,
                        principalTable: "LeagueGender",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // One club per league and club id (per league and name for rosters without an id), named after its newest
            // roster; the foreign key below fails on the empty default. Manual merges are applied by the Importer.
            migrationBuilder.Sql(
                """
                CREATE TEMPORARY TABLE "ClubBackfill" AS
                SELECT uuidv7() AS "ClubId", g.*
                FROM (
                    SELECT DISTINCT ON (r."LeagueId", r."ExternalId", CASE WHEN r."ExternalId" IS NULL THEN r."NormalizedName" END)
                           r."LeagueId",
                           r."ExternalId",
                           CASE WHEN r."ExternalId" IS NULL THEN r."NormalizedName" END AS "GroupName",
                           r."Name",
                           r."NormalizedName",
                           l."GenderId"
                    FROM "Roster" AS r
                    JOIN "Season" AS s ON s."Id" = r."SeasonId"
                    JOIN "League" AS l ON l."Id" = r."LeagueId"
                    ORDER BY r."LeagueId", r."ExternalId", CASE WHEN r."ExternalId" IS NULL THEN r."NormalizedName" END, s."StartYear" DESC
                ) AS g;

                INSERT INTO "Club" ("Id", "Name", "NormalizedName", "GenderId")
                SELECT "ClubId", "Name", "NormalizedName", "GenderId"
                FROM "ClubBackfill";

                UPDATE "Roster" AS r
                SET "ClubId" = b."ClubId"
                FROM "ClubBackfill" AS b
                WHERE b."LeagueId" = r."LeagueId"
                  AND (b."ExternalId" = r."ExternalId"
                       OR (r."ExternalId" IS NULL AND b."ExternalId" IS NULL AND b."GroupName" = r."NormalizedName"));

                DROP TABLE "ClubBackfill";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Roster_ClubId",
                table: "Roster",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Club_GenderId",
                table: "Club",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Club_NormalizedName",
                table: "Club",
                column: "NormalizedName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.AddForeignKey(
                name: "FK_Roster_Club_ClubId",
                table: "Roster",
                column: "ClubId",
                principalTable: "Club",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roster_Club_ClubId",
                table: "Roster");

            migrationBuilder.DropTable(
                name: "Club");

            migrationBuilder.DropIndex(
                name: "IX_Roster_ClubId",
                table: "Roster");

            migrationBuilder.DropColumn(
                name: "ClubId",
                table: "Roster");
        }
    }
}
