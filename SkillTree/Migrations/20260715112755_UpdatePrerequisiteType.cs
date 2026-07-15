using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace skill_tree.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrerequisiteType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prerequisites_Prerequisites_PrerequisiteId",
                table: "Prerequisites");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "SkillLogs",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Prerequisites_Skills_PrerequisiteId",
                table: "Prerequisites",
                column: "PrerequisiteId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prerequisites_Skills_PrerequisiteId",
                table: "Prerequisites");

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "SkillLogs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddForeignKey(
                name: "FK_Prerequisites_Prerequisites_PrerequisiteId",
                table: "Prerequisites",
                column: "PrerequisiteId",
                principalTable: "Prerequisites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
