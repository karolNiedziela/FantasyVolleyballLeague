using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FantasyVolleyballLeague.VolleyballData.Database.Migrations
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
                name: "LeagueGender",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueGender", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    ExternalId = table.Column<int>(type: "integer", nullable: true),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    HeightCm = table.Column<int>(type: "integer", nullable: true),
                    WeightKg = table.Column<int>(type: "integer", nullable: true),
                    AttackReachCm = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
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
                name: "StatisticsLevel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticsLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "League",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DivisionNumber = table.Column<short>(type: "smallint", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    GenderId = table.Column<int>(type: "integer", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_League_LeagueGender_GenderId",
                        column: x => x.GenderId,
                        principalTable: "LeagueGender",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Season",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartYear = table.Column<int>(type: "integer", nullable: false),
                    EndYear = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<int>(type: "integer", nullable: true),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Season", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Season_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Phase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phase_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roster",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalId = table.Column<int>(type: "integer", nullable: true),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roster_League_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "League",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Roster_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Round",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    Stage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StageOrder = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    PhaseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Round", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Round_Phase_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "Phase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RosterPlayer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    RosterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShirtNumber = table.Column<int>(type: "integer", nullable: true),
                    PositionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterPlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RosterPlayer_PlayerPosition_PositionId",
                        column: x => x.PositionId,
                        principalTable: "PlayerPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RosterPlayer_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RosterPlayer_Roster_RosterId",
                        column: x => x.RosterId,
                        principalTable: "Roster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    ExternalId = table.Column<int>(type: "integer", nullable: true),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsTimeConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    StatisticsLevelId = table.Column<int>(type: "integer", nullable: false),
                    MatchNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Attendance = table.Column<int>(type: "integer", nullable: true),
                    SourceScrapedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    MvpId = table.Column<Guid>(type: "uuid", nullable: true),
                    HomeRosterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayRosterId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    Officials_Commissioner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Officials_FirstReferee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Officials_SecondReferee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VenueCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VenueName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
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
                        name: "FK_Match_MatchStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "MatchStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Player_MvpId",
                        column: x => x.MvpId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Roster_AwayRosterId",
                        column: x => x.AwayRosterId,
                        principalTable: "Roster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Roster_HomeRosterId",
                        column: x => x.HomeRosterId,
                        principalTable: "Roster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Round_RoundId",
                        column: x => x.RoundId,
                        principalTable: "Round",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_StatisticsLevel_StatisticsLevelId",
                        column: x => x.StatisticsLevelId,
                        principalTable: "StatisticsLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMatchStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    Number = table.Column<int>(type: "integer", nullable: true),
                    PointsTotal = table.Column<int>(type: "integer", nullable: true),
                    ServeTotal = table.Column<int>(type: "integer", nullable: true),
                    ServeErrors = table.Column<int>(type: "integer", nullable: true),
                    Aces = table.Column<int>(type: "integer", nullable: true),
                    ReceptionTotal = table.Column<int>(type: "integer", nullable: true),
                    ReceptionErrors = table.Column<int>(type: "integer", nullable: true),
                    ReceptionPositivePercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ReceptionPerfectPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    AttackTotal = table.Column<int>(type: "integer", nullable: true),
                    AttackErrors = table.Column<int>(type: "integer", nullable: true),
                    AttackBlocked = table.Column<int>(type: "integer", nullable: true),
                    AttackPoints = table.Column<int>(type: "integer", nullable: true),
                    AttackEfficiencyPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Blocks = table.Column<int>(type: "integer", nullable: true),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RosterId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMatchStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStatistics_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStatistics_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStatistics_Roster_RosterId",
                        column: x => x.RosterId,
                        principalTable: "Roster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SetScore",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false),
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
                table: "LeagueGender",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Men" },
                    { 2, "Women" }
                });

            migrationBuilder.InsertData(
                table: "MatchStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Scheduled" },
                    { 2, "Finished" }
                });

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

            migrationBuilder.InsertData(
                table: "StatisticsLevel",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "None" },
                    { 2, "LineupOnly" },
                    { 3, "Full" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_League_CountryId_GenderId_DivisionNumber",
                table: "League",
                columns: new[] { "CountryId", "GenderId", "DivisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_League_GenderId",
                table: "League",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_AwayRosterId",
                table: "Match",
                column: "AwayRosterId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_ExternalId_LeagueId",
                table: "Match",
                columns: new[] { "ExternalId", "LeagueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Match_HomeRosterId",
                table: "Match",
                column: "HomeRosterId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_LeagueId",
                table: "Match",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_MvpId",
                table: "Match",
                column: "MvpId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_RoundId",
                table: "Match",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_StatisticsLevelId",
                table: "Match",
                column: "StatisticsLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_StatusId",
                table: "Match",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Phase_SeasonId_Name",
                table: "Phase",
                columns: new[] { "SeasonId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Player_ExternalId",
                table: "Player",
                column: "ExternalId",
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStatistics_MatchId_PlayerId",
                table: "PlayerMatchStatistics",
                columns: new[] { "MatchId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStatistics_PlayerId",
                table: "PlayerMatchStatistics",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStatistics_RosterId",
                table: "PlayerMatchStatistics",
                column: "RosterId");

            migrationBuilder.CreateIndex(
                name: "IX_Roster_LeagueId",
                table: "Roster",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Roster_Name_LeagueId_SeasonId",
                table: "Roster",
                columns: new[] { "Name", "LeagueId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roster_SeasonId_ExternalId",
                table: "Roster",
                columns: new[] { "SeasonId", "ExternalId" },
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RosterPlayer_PlayerId",
                table: "RosterPlayer",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_RosterPlayer_PositionId",
                table: "RosterPlayer",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_RosterPlayer_RosterId_PlayerId",
                table: "RosterPlayer",
                columns: new[] { "RosterId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Round_PhaseId_Stage_Name",
                table: "Round",
                columns: new[] { "PhaseId", "Stage", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Season_LeagueId_StartYear_EndYear",
                table: "Season",
                columns: new[] { "LeagueId", "StartYear", "EndYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetScore_MatchId_SetNumber",
                table: "SetScore",
                columns: new[] { "MatchId", "SetNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerMatchStatistics");

            migrationBuilder.DropTable(
                name: "RosterPlayer");

            migrationBuilder.DropTable(
                name: "SetScore");

            migrationBuilder.DropTable(
                name: "PlayerPosition");

            migrationBuilder.DropTable(
                name: "Match");

            migrationBuilder.DropTable(
                name: "MatchStatus");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "Roster");

            migrationBuilder.DropTable(
                name: "Round");

            migrationBuilder.DropTable(
                name: "StatisticsLevel");

            migrationBuilder.DropTable(
                name: "Phase");

            migrationBuilder.DropTable(
                name: "Season");

            migrationBuilder.DropTable(
                name: "League");

            migrationBuilder.DropTable(
                name: "LeagueCountry");

            migrationBuilder.DropTable(
                name: "LeagueGender");
        }
    }
}
