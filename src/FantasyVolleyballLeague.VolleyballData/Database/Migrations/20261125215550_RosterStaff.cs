using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FantasyVolleyballLeague.VolleyballData.Database.Migrations
{
    /// <inheritdoc />
    public partial class RosterStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "pl-x-icu"),
                    NormalizedName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMember", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaffRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RosterStaffMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    RosterId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterStaffMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RosterStaffMember_Roster_RosterId",
                        column: x => x.RosterId,
                        principalTable: "Roster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RosterStaffMember_StaffMember_StaffMemberId",
                        column: x => x.StaffMemberId,
                        principalTable: "StaffMember",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RosterStaffMember_StaffRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "StaffRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "StaffRole",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "head_coach", "Head Coach" },
                    { 2, "second_coach", "Second Coach" },
                    { 3, "third_coach", "Third Coach" },
                    { 4, "president", "President" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RosterStaffMember_RoleId",
                table: "RosterStaffMember",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RosterStaffMember_RosterId_StaffMemberId_RoleId",
                table: "RosterStaffMember",
                columns: new[] { "RosterId", "StaffMemberId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RosterStaffMember_StaffMemberId",
                table: "RosterStaffMember",
                column: "StaffMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMember_NormalizedName",
                table: "StaffMember",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffMember_NormalizedName_Trigram",
                table: "StaffMember",
                column: "NormalizedName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffRole_Code",
                table: "StaffRole",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RosterStaffMember");

            migrationBuilder.DropTable(
                name: "StaffMember");

            migrationBuilder.DropTable(
                name: "StaffRole");
        }
    }
}
