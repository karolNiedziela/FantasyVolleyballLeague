using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyVolleyballLeague.VolleyballData.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeasonLeagueName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeagueName",
                table: "Season",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "pl-x-icu");

            // Existing seasons start with the current league name; the Importer then applies ManualData/league-names.json.
            migrationBuilder.Sql(
                """
                UPDATE "Season" AS s
                SET "LeagueName" = l."Name"
                FROM "League" AS l
                WHERE l."Id" = s."LeagueId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeagueName",
                table: "Season");
        }
    }
}
