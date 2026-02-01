using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prediction_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictedProb = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    AiProb = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    PhysicsProb = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualWaterLevel = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: true),
                    AccuracyScore = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prediction_logs", x => x.Id);
                    table.CheckConstraint(
     "chk_prob_range",
     "\"PredictedProb\" >= 0 AND \"PredictedProb\" <= 1");

                    table.CheckConstraint(
                        "chk_time_range",
                        "\"EndTime\" > \"StartTime\"");
                    table.ForeignKey(
                        name: "FK_prediction_logs_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 1, 13, 26, 4, 476, DateTimeKind.Utc).AddTicks(5745), new DateTime(2026, 2, 1, 13, 26, 4, 476, DateTimeKind.Utc).AddTicks(5745) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 1, 13, 26, 4, 476, DateTimeKind.Utc).AddTicks(5745), new DateTime(2026, 2, 1, 13, 26, 4, 476, DateTimeKind.Utc).AddTicks(5745) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 1, 13, 26, 4, 476, DateTimeKind.Utc).AddTicks(5745), new DateTime(2026, 2, 1, 13, 26, 4, 476, DateTimeKind.Utc).AddTicks(5745) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 1, 13, 26, 4, 476, DateTimeKind.Utc).AddTicks(5745), new DateTime(2026, 2, 1, 13, 26, 4, 476, DateTimeKind.Utc).AddTicks(5745) });

            migrationBuilder.CreateIndex(
                name: "ix_prediction_logs_area_time",
                table: "prediction_logs",
                columns: new[] { "AreaId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "ix_prediction_logs_end_time",
                table: "prediction_logs",
                column: "EndTime",
                filter: "\"IsVerified\" = false");

            migrationBuilder.CreateIndex(
                name: "ix_prediction_logs_verified",
                table: "prediction_logs",
                columns: new[] { "IsVerified", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prediction_logs");

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 17, 16, 53, 488, DateTimeKind.Utc).AddTicks(5070), new DateTime(2026, 1, 28, 17, 16, 53, 488, DateTimeKind.Utc).AddTicks(5070) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 17, 16, 53, 488, DateTimeKind.Utc).AddTicks(5070), new DateTime(2026, 1, 28, 17, 16, 53, 488, DateTimeKind.Utc).AddTicks(5070) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 17, 16, 53, 488, DateTimeKind.Utc).AddTicks(5070), new DateTime(2026, 1, 28, 17, 16, 53, 488, DateTimeKind.Utc).AddTicks(5070) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 17, 16, 53, 488, DateTimeKind.Utc).AddTicks(5070), new DateTime(2026, 1, 28, 17, 16, 53, 488, DateTimeKind.Utc).AddTicks(5070) });
        }
    }
}
