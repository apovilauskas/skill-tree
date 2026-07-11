using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace skill_tree.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prerequisites_Skills_PrerequisiteId",
                table: "Prerequisites");

            migrationBuilder.AddColumn<string>(
                name: "Metric",
                table: "Skills",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Target",
                table: "Skills",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SkillLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillLogs_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillLogs_SkillId",
                table: "SkillLogs",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prerequisites_Prerequisites_PrerequisiteId",
                table: "Prerequisites",
                column: "PrerequisiteId",
                principalTable: "Prerequisites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prerequisites_Prerequisites_PrerequisiteId",
                table: "Prerequisites");

            migrationBuilder.DropTable(
                name: "SkillLogs");

            migrationBuilder.DropColumn(
                name: "Metric",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "Skills");

            migrationBuilder.AddForeignKey(
                name: "FK_Prerequisites_Skills_PrerequisiteId",
                table: "Prerequisites",
                column: "PrerequisiteId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
