using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FantasyVolleyballLeague.Api.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeagueCountry",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueCountry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerPosition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerPosition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "League",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DivisionNumber = table.Column<short>(type: "smallint", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_League", x => x.Id);
                    table.CheckConstraint("CK_Leagues_DivisionNumber", "\"DivisionNumber\" >= 1");
                    table.ForeignKey(
                        name: "FK_League_LeagueCountry_CountryId",
                        column: x => x.CountryId,
                        principalTable: "LeagueCountry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Team_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PositionId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Player_PlayerPosition_PositionId",
                        column: x => x.PositionId,
                        principalTable: "PlayerPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Player_Team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExternalId = table.Column<int>(type: "integer", nullable: true),
                    VenueName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VenueCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Officials_FirstReferee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Officials_SecondReferee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Officials_Commissioner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    MvpId = table.Column<Guid>(type: "uuid", nullable: true),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Match", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Match_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Player_MvpId",
                        column: x => x.MvpId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Team_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Team_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMatchStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    PointsTotal = table.Column<int>(type: "integer", nullable: false),
                    ServeTotal = table.Column<int>(type: "integer", nullable: false),
                    ServeErrors = table.Column<int>(type: "integer", nullable: false),
                    Aces = table.Column<int>(type: "integer", nullable: false),
                    ReceptionTotal = table.Column<int>(type: "integer", nullable: false),
                    ReceptionErrors = table.Column<int>(type: "integer", nullable: false),
                    ReceptionPositivePercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ReceptionPerfectPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    AttackTotal = table.Column<int>(type: "integer", nullable: false),
                    AttackErrors = table.Column<int>(type: "integer", nullable: false),
                    AttackBlocked = table.Column<int>(type: "integer", nullable: false),
                    AttackPoints = table.Column<int>(type: "integer", nullable: false),
                    AttackEfficiencyPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Blocks = table.Column<int>(type: "integer", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMatchStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStatistics_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStatistics_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SetScore",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    HomeTeamScore = table.Column<int>(type: "integer", nullable: false),
                    AwayTeamScore = table.Column<int>(type: "integer", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetScore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetScore_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LeagueCountry",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Poland" });

            migrationBuilder.InsertData(
                table: "PlayerPosition",
                columns: new[] { "Id", "Abbreviation", "Name" },
                values: new object[,]
                {
                    { 1, "OH", "Outside Hitter" },
                    { 2, "OPP", "Opposite Hitter" },
                    { 3, "MB", "Middle Blocker" },
                    { 4, "S", "Setter" },
                    { 5, "L", "Libero" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_League_CountryId",
                table: "League",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_AwayTeamId",
                table: "Match",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_ExternalId_LeagueId",
                table: "Match",
                columns: new[] { "ExternalId", "LeagueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Match_HomeTeamId",
                table: "Match",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_LeagueId",
                table: "Match",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_MvpId",
                table: "Match",
                column: "MvpId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_PositionId",
                table: "Player",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_TeamId",
                table: "Player",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStatistics_MatchId",
                table: "PlayerMatchStatistics",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStatistics_PlayerId",
                table: "PlayerMatchStatistics",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SetScore_MatchId_SetNumber",
                table: "SetScore",
                columns: new[] { "MatchId", "SetNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_LeagueId",
                table: "Team",
                column: "LeagueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerMatchStatistics");

            migrationBuilder.DropTable(
                name: "SetScore");

            migrationBuilder.DropTable(
                name: "Match");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "PlayerPosition");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropTable(
                name: "League");

            migrationBuilder.DropTable(
                name: "LeagueCountry");
        }
    }
}
