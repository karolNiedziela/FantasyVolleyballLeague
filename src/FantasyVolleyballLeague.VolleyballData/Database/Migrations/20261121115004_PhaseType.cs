using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FantasyVolleyballLeague.VolleyballData.Database.Migrations
{
    /// <inheritdoc />
    public partial class PhaseType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhaseTypeId",
                table: "Phase",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "PhaseType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseType", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PhaseType",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "regular_season", "Regular Season" },
                    { 2, "playoff", "Playoff" },
                    { 3, "relegation", "Relegation" }
                });

            // Existing phases got the default (1, regular season) above. The Importer only ever wrote these
            // three names, so the other two types are set from the name.
            migrationBuilder.Sql("""UPDATE "Phase" SET "PhaseTypeId" = 2 WHERE "Name" = 'Playoff';""");
            migrationBuilder.Sql("""UPDATE "Phase" SET "PhaseTypeId" = 3 WHERE "Name" = 'Relegation';""");

            migrationBuilder.CreateIndex(
                name: "IX_Phase_PhaseTypeId",
                table: "Phase",
                column: "PhaseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseType_Code",
                table: "PhaseType",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Phase_PhaseType_PhaseTypeId",
                table: "Phase",
                column: "PhaseTypeId",
                principalTable: "PhaseType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Phase_PhaseType_PhaseTypeId",
                table: "Phase");

            migrationBuilder.DropTable(
                name: "PhaseType");

            migrationBuilder.DropIndex(
                name: "IX_Phase_PhaseTypeId",
                table: "Phase");

            migrationBuilder.DropColumn(
                name: "PhaseTypeId",
                table: "Phase");
        }
    }
}
