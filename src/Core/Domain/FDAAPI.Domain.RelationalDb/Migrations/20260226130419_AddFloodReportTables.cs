using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddFloodReportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flood_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "medium", comment: "low | medium | high"),
                    TrustScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 50, comment: "0-100"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "published", comment: "published | hidden | escalated"),
                    ConfidenceLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "medium", comment: "low | medium | high"),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "normal", comment: "normal | high | critical"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flood_reports", x => x.Id);
                    table.CheckConstraint("chk_priority", "\"Priority\" IN ('normal', 'high', 'critical')");
                    table.CheckConstraint("chk_severity", "\"Severity\" IN ('low', 'medium', 'high')");
                    table.CheckConstraint("chk_status", "\"Status\" IN ('published', 'hidden', 'escalated')");
                    table.CheckConstraint("chk_trust_score", "\"TrustScore\" >= 0 AND \"TrustScore\" <= 100");
                    table.ForeignKey(
                        name: "FK_flood_reports_Users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "flood_report_flags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "spam | fake | inappropriate"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flood_report_flags", x => x.Id);
                    table.CheckConstraint("chk_flag_reason", "\"Reason\" IN ('spam', 'fake', 'inappropriate')");
                    table.ForeignKey(
                        name: "FK_flood_report_flags_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flood_report_flags_flood_reports_FloodReportId",
                        column: x => x.FloodReportId,
                        principalTable: "flood_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flood_report_media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "photo", comment: "photo | video"),
                    MediaUrl = table.Column<string>(type: "text", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flood_report_media", x => x.Id);
                    table.CheckConstraint("chk_media_type", "\"MediaType\" IN ('photo', 'video')");
                    table.ForeignKey(
                        name: "FK_flood_report_media_flood_reports_FloodReportId",
                        column: x => x.FloodReportId,
                        principalTable: "flood_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flood_report_votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoteType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "up", comment: "up | down"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flood_report_votes", x => x.Id);
                    table.CheckConstraint("chk_vote_type", "\"VoteType\" IN ('up', 'down')");
                    table.ForeignKey(
                        name: "FK_flood_report_votes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flood_report_votes_flood_reports_FloodReportId",
                        column: x => x.FloodReportId,
                        principalTable: "flood_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "ix_flood_report_flags_report",
                table: "flood_report_flags",
                column: "FloodReportId");

            migrationBuilder.CreateIndex(
                name: "ix_flood_report_flags_user",
                table: "flood_report_flags",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "uq_flag",
                table: "flood_report_flags",
                columns: new[] { "FloodReportId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_flood_report_media_report",
                table: "flood_report_media",
                column: "FloodReportId");

            migrationBuilder.CreateIndex(
                name: "ix_flood_report_votes_report",
                table: "flood_report_votes",
                column: "FloodReportId");

            migrationBuilder.CreateIndex(
                name: "ix_flood_report_votes_user",
                table: "flood_report_votes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "uq_vote",
                table: "flood_report_votes",
                columns: new[] { "FloodReportId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_flood_reports_created",
                table: "flood_reports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "ix_flood_reports_location",
                table: "flood_reports",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "ix_flood_reports_priority",
                table: "flood_reports",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "ix_flood_reports_reporter",
                table: "flood_reports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "ix_flood_reports_status",
                table: "flood_reports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "ix_flood_reports_trust_score",
                table: "flood_reports",
                column: "TrustScore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flood_report_flags");

            migrationBuilder.DropTable(
                name: "flood_report_media");

            migrationBuilder.DropTable(
                name: "flood_report_votes");

            migrationBuilder.DropTable(
                name: "flood_reports");

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 5, 14, 10, 41, 261, DateTimeKind.Utc).AddTicks(6384), new DateTime(2026, 2, 5, 14, 10, 41, 261, DateTimeKind.Utc).AddTicks(6384) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 5, 14, 10, 41, 261, DateTimeKind.Utc).AddTicks(6384), new DateTime(2026, 2, 5, 14, 10, 41, 261, DateTimeKind.Utc).AddTicks(6384) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 5, 14, 10, 41, 261, DateTimeKind.Utc).AddTicks(6384), new DateTime(2026, 2, 5, 14, 10, 41, 261, DateTimeKind.Utc).AddTicks(6384) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 5, 14, 10, 41, 261, DateTimeKind.Utc).AddTicks(6384), new DateTime(2026, 2, 5, 14, 10, 41, 261, DateTimeKind.Utc).AddTicks(6384) });
        }
    }
}
