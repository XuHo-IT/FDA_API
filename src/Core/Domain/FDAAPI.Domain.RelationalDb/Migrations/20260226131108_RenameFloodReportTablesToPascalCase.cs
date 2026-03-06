using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class RenameFloodReportTablesToPascalCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_flood_report_flags_Users_UserId",
                table: "flood_report_flags");

            migrationBuilder.DropForeignKey(
                name: "FK_flood_report_flags_flood_reports_FloodReportId",
                table: "flood_report_flags");

            migrationBuilder.DropForeignKey(
                name: "FK_flood_report_media_flood_reports_FloodReportId",
                table: "flood_report_media");

            migrationBuilder.DropForeignKey(
                name: "FK_flood_report_votes_Users_UserId",
                table: "flood_report_votes");

            migrationBuilder.DropForeignKey(
                name: "FK_flood_report_votes_flood_reports_FloodReportId",
                table: "flood_report_votes");

            migrationBuilder.DropForeignKey(
                name: "FK_flood_reports_Users_ReporterUserId",
                table: "flood_reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_flood_reports",
                table: "flood_reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_flood_report_votes",
                table: "flood_report_votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_flood_report_media",
                table: "flood_report_media");

            migrationBuilder.DropPrimaryKey(
                name: "PK_flood_report_flags",
                table: "flood_report_flags");

            migrationBuilder.RenameTable(
                name: "flood_reports",
                newName: "FloodReports");

            migrationBuilder.RenameTable(
                name: "flood_report_votes",
                newName: "FloodReportVotes");

            migrationBuilder.RenameTable(
                name: "flood_report_media",
                newName: "FloodReportMedia");

            migrationBuilder.RenameTable(
                name: "flood_report_flags",
                newName: "FloodReportFlags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FloodReports",
                table: "FloodReports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FloodReportVotes",
                table: "FloodReportVotes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FloodReportMedia",
                table: "FloodReportMedia",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FloodReportFlags",
                table: "FloodReportFlags",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 11, 6, 784, DateTimeKind.Utc).AddTicks(213), new DateTime(2026, 2, 26, 13, 11, 6, 784, DateTimeKind.Utc).AddTicks(213) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 11, 6, 784, DateTimeKind.Utc).AddTicks(213), new DateTime(2026, 2, 26, 13, 11, 6, 784, DateTimeKind.Utc).AddTicks(213) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 11, 6, 784, DateTimeKind.Utc).AddTicks(213), new DateTime(2026, 2, 26, 13, 11, 6, 784, DateTimeKind.Utc).AddTicks(213) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 11, 6, 784, DateTimeKind.Utc).AddTicks(213), new DateTime(2026, 2, 26, 13, 11, 6, 784, DateTimeKind.Utc).AddTicks(213) });

            migrationBuilder.AddForeignKey(
                name: "FK_FloodReportFlags_FloodReports_FloodReportId",
                table: "FloodReportFlags",
                column: "FloodReportId",
                principalTable: "FloodReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloodReportFlags_Users_UserId",
                table: "FloodReportFlags",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloodReportMedia_FloodReports_FloodReportId",
                table: "FloodReportMedia",
                column: "FloodReportId",
                principalTable: "FloodReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloodReports_Users_ReporterUserId",
                table: "FloodReports",
                column: "ReporterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FloodReportVotes_FloodReports_FloodReportId",
                table: "FloodReportVotes",
                column: "FloodReportId",
                principalTable: "FloodReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloodReportVotes_Users_UserId",
                table: "FloodReportVotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloodReportFlags_FloodReports_FloodReportId",
                table: "FloodReportFlags");

            migrationBuilder.DropForeignKey(
                name: "FK_FloodReportFlags_Users_UserId",
                table: "FloodReportFlags");

            migrationBuilder.DropForeignKey(
                name: "FK_FloodReportMedia_FloodReports_FloodReportId",
                table: "FloodReportMedia");

            migrationBuilder.DropForeignKey(
                name: "FK_FloodReports_Users_ReporterUserId",
                table: "FloodReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FloodReportVotes_FloodReports_FloodReportId",
                table: "FloodReportVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_FloodReportVotes_Users_UserId",
                table: "FloodReportVotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FloodReportVotes",
                table: "FloodReportVotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FloodReports",
                table: "FloodReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FloodReportMedia",
                table: "FloodReportMedia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FloodReportFlags",
                table: "FloodReportFlags");

            migrationBuilder.RenameTable(
                name: "FloodReportVotes",
                newName: "flood_report_votes");

            migrationBuilder.RenameTable(
                name: "FloodReports",
                newName: "flood_reports");

            migrationBuilder.RenameTable(
                name: "FloodReportMedia",
                newName: "flood_report_media");

            migrationBuilder.RenameTable(
                name: "FloodReportFlags",
                newName: "flood_report_flags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_flood_report_votes",
                table: "flood_report_votes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_flood_reports",
                table: "flood_reports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_flood_report_media",
                table: "flood_report_media",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_flood_report_flags",
                table: "flood_report_flags",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 4, 17, 315, DateTimeKind.Utc).AddTicks(5958), new DateTime(2026, 2, 26, 13, 4, 17, 315, DateTimeKind.Utc).AddTicks(5958) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 4, 17, 315, DateTimeKind.Utc).AddTicks(5958), new DateTime(2026, 2, 26, 13, 4, 17, 315, DateTimeKind.Utc).AddTicks(5958) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 4, 17, 315, DateTimeKind.Utc).AddTicks(5958), new DateTime(2026, 2, 26, 13, 4, 17, 315, DateTimeKind.Utc).AddTicks(5958) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 26, 13, 4, 17, 315, DateTimeKind.Utc).AddTicks(5958), new DateTime(2026, 2, 26, 13, 4, 17, 315, DateTimeKind.Utc).AddTicks(5958) });

            migrationBuilder.AddForeignKey(
                name: "FK_flood_report_flags_Users_UserId",
                table: "flood_report_flags",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_flood_report_flags_flood_reports_FloodReportId",
                table: "flood_report_flags",
                column: "FloodReportId",
                principalTable: "flood_reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_flood_report_media_flood_reports_FloodReportId",
                table: "flood_report_media",
                column: "FloodReportId",
                principalTable: "flood_reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_flood_report_votes_Users_UserId",
                table: "flood_report_votes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_flood_report_votes_flood_reports_FloodReportId",
                table: "flood_report_votes",
                column: "FloodReportId",
                principalTable: "flood_reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_flood_reports_Users_ReporterUserId",
                table: "flood_reports",
                column: "ReporterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
