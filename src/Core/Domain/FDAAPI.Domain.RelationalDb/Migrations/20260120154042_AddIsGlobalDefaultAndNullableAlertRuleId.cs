using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddIsGlobalDefaultAndNullableAlertRuleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make AlertRuleId nullable in Alerts table
            migrationBuilder.AlterColumn<Guid>(
                name: "AlertRuleId",
                table: "Alerts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Add IsGlobalDefault column to AlertRules table
            migrationBuilder.AddColumn<bool>(
                name: "IsGlobalDefault",
                table: "AlertRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove IsGlobalDefault column from AlertRules
            migrationBuilder.DropColumn(
                name: "IsGlobalDefault",
                table: "AlertRules");

            // Revert AlertRuleId to non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "AlertRuleId",
                table: "Alerts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
