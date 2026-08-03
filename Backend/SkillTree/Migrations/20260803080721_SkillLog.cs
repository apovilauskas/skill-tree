using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace skill_tree.Migrations
{
    /// <inheritdoc />
    public partial class SkillLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SkillLogs_UserId",
                table: "SkillLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SkillLogs_AspNetUsers_UserId",
                table: "SkillLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkillProgresses_AspNetUsers_UserId",
                table: "UserSkillProgresses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkillLogs_AspNetUsers_UserId",
                table: "SkillLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSkillProgresses_AspNetUsers_UserId",
                table: "UserSkillProgresses");

            migrationBuilder.DropIndex(
                name: "IX_SkillLogs_UserId",
                table: "SkillLogs");
        }
    }
}
