using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class updateUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_preferences_Users_UserId",
                table: "user_preferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_preferences",
                table: "user_preferences");

            migrationBuilder.RenameTable(
                name: "user_preferences",
                newName: "UserPreferences");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPreferences",
                table: "UserPreferences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_Users_UserId",
                table: "UserPreferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_Users_UserId",
                table: "UserPreferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPreferences",
                table: "UserPreferences");

            migrationBuilder.RenameTable(
                name: "UserPreferences",
                newName: "user_preferences");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_preferences",
                table: "user_preferences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_preferences_Users_UserId",
                table: "user_preferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
