using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace skill_tree.Migrations
{
    /// <inheritdoc />
    public partial class MultiUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Skills");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "UserSkillProgresses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "UserSkillProgresses",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "UserSkillProgresses");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "UserSkillProgresses");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Skills",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Skills",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
