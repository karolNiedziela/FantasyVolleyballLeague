using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyVolleyballLeague.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Match_ExternalMatchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExternalMatchId",
                table: "Match",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Match_ExternalMatchId",
                table: "Match",
                column: "ExternalMatchId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Match_ExternalMatchId",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "ExternalMatchId",
                table: "Match");
        }
    }
}
