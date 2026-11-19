using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyVolleyballLeague.VolleyballData.Database.Migrations
{
    /// <inheritdoc />
    public partial class LookupCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "StatisticsLevel",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PlayerPosition",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "MatchStatus",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "LeagueGender",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "LeagueCountry",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "LeagueCountry",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "pl");

            migrationBuilder.UpdateData(
                table: "LeagueGender",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "men");

            migrationBuilder.UpdateData(
                table: "LeagueGender",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "women");

            migrationBuilder.UpdateData(
                table: "MatchStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "scheduled");

            migrationBuilder.UpdateData(
                table: "MatchStatus",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "finished");

            migrationBuilder.UpdateData(
                table: "PlayerPosition",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "outside_hitter");

            migrationBuilder.UpdateData(
                table: "PlayerPosition",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "opposite_hitter");

            migrationBuilder.UpdateData(
                table: "PlayerPosition",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: "middle_blocker");

            migrationBuilder.UpdateData(
                table: "PlayerPosition",
                keyColumn: "Id",
                keyValue: 4,
                column: "Code",
                value: "setter");

            migrationBuilder.UpdateData(
                table: "PlayerPosition",
                keyColumn: "Id",
                keyValue: 5,
                column: "Code",
                value: "libero");

            migrationBuilder.UpdateData(
                table: "StatisticsLevel",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "none");

            migrationBuilder.UpdateData(
                table: "StatisticsLevel",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "lineup_only");

            migrationBuilder.UpdateData(
                table: "StatisticsLevel",
                keyColumn: "Id",
                keyValue: 3,
                column: "Code",
                value: "full");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsLevel_Code",
                table: "StatisticsLevel",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPosition_Code",
                table: "PlayerPosition",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchStatus_Code",
                table: "MatchStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueGender_Code",
                table: "LeagueGender",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueCountry_Code",
                table: "LeagueCountry",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StatisticsLevel_Code",
                table: "StatisticsLevel");

            migrationBuilder.DropIndex(
                name: "IX_PlayerPosition_Code",
                table: "PlayerPosition");

            migrationBuilder.DropIndex(
                name: "IX_MatchStatus_Code",
                table: "MatchStatus");

            migrationBuilder.DropIndex(
                name: "IX_LeagueGender_Code",
                table: "LeagueGender");

            migrationBuilder.DropIndex(
                name: "IX_LeagueCountry_Code",
                table: "LeagueCountry");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "StatisticsLevel");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PlayerPosition");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "MatchStatus");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "LeagueGender");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "LeagueCountry");
        }
    }
}
