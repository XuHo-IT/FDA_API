using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsAggregationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AdministrativeAreaId",
                table: "stations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdministrativeAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Geometry = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrativeAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdministrativeAreas_AdministrativeAreas_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AdministrativeAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Schedule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloodAnalyticsFrequency",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministrativeAreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeBucket = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BucketType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExceedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodAnalyticsFrequency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloodAnalyticsFrequency_AdministrativeAreas_AdministrativeA~",
                        column: x => x.AdministrativeAreaId,
                        principalTable: "AdministrativeAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FloodAnalyticsHotspots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministrativeAreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodAnalyticsHotspots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloodAnalyticsHotspots_AdministrativeAreas_AdministrativeAr~",
                        column: x => x.AdministrativeAreaId,
                        principalTable: "AdministrativeAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FloodAnalyticsSeverity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministrativeAreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeBucket = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BucketType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaxLevel = table.Column<decimal>(type: "numeric(14,4)", nullable: true),
                    AvgLevel = table.Column<decimal>(type: "numeric(14,4)", nullable: true),
                    MinLevel = table.Column<decimal>(type: "numeric(14,4)", nullable: true),
                    DurationHours = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReadingCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodAnalyticsSeverity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloodAnalyticsSeverity_AdministrativeAreas_AdministrativeAr~",
                        column: x => x.AdministrativeAreaId,
                        principalTable: "AdministrativeAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FloodEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministrativeAreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeakLevel = table.Column<decimal>(type: "numeric(14,4)", nullable: true),
                    DurationHours = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloodEvents_AdministrativeAreas_AdministrativeAreaId",
                        column: x => x.AdministrativeAreaId,
                        principalTable: "AdministrativeAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsJobRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RecordsProcessed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RecordsCreated = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExecutionTimeMs = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsJobRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsJobRuns_AnalyticsJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "AnalyticsJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_station_administrative_area",
                table: "stations",
                column: "AdministrativeAreaId");

            migrationBuilder.CreateIndex(
                name: "ix_administrative_areas_code",
                table: "AdministrativeAreas",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "ix_administrative_areas_level",
                table: "AdministrativeAreas",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "ix_administrative_areas_parent",
                table: "AdministrativeAreas",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "ix_job_runs_job",
                table: "AnalyticsJobRuns",
                columns: new[] { "JobId", "StartedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_job_runs_status",
                table: "AnalyticsJobRuns",
                columns: new[] { "Status", "StartedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_jobs_active",
                table: "AnalyticsJobs",
                columns: new[] { "IsActive", "NextRunAt" });

            migrationBuilder.CreateIndex(
                name: "ix_jobs_type",
                table: "AnalyticsJobs",
                column: "JobType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_frequency_area_bucket",
                table: "FloodAnalyticsFrequency",
                columns: new[] { "AdministrativeAreaId", "TimeBucket" });

            migrationBuilder.CreateIndex(
                name: "ix_frequency_bucket_type",
                table: "FloodAnalyticsFrequency",
                columns: new[] { "BucketType", "TimeBucket" });

            migrationBuilder.CreateIndex(
                name: "uq_frequency_area_bucket",
                table: "FloodAnalyticsFrequency",
                columns: new[] { "AdministrativeAreaId", "TimeBucket", "BucketType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hotspot_area",
                table: "FloodAnalyticsHotspots",
                column: "AdministrativeAreaId");

            migrationBuilder.CreateIndex(
                name: "ix_hotspot_score",
                table: "FloodAnalyticsHotspots",
                columns: new[] { "Score", "CalculatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "uq_hotspot_area_period",
                table: "FloodAnalyticsHotspots",
                columns: new[] { "AdministrativeAreaId", "PeriodStart", "PeriodEnd" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_severity_area_bucket",
                table: "FloodAnalyticsSeverity",
                columns: new[] { "AdministrativeAreaId", "TimeBucket" });

            migrationBuilder.CreateIndex(
                name: "ix_severity_bucket_type",
                table: "FloodAnalyticsSeverity",
                columns: new[] { "BucketType", "TimeBucket" });

            migrationBuilder.CreateIndex(
                name: "uq_severity_area_bucket",
                table: "FloodAnalyticsSeverity",
                columns: new[] { "AdministrativeAreaId", "TimeBucket", "BucketType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_flood_events_area_start",
                table: "FloodEvents",
                columns: new[] { "AdministrativeAreaId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "ix_flood_events_start_time",
                table: "FloodEvents",
                column: "StartTime");

            migrationBuilder.AddForeignKey(
                name: "FK_stations_AdministrativeAreas_AdministrativeAreaId",
                table: "stations",
                column: "AdministrativeAreaId",
                principalTable: "AdministrativeAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stations_AdministrativeAreas_AdministrativeAreaId",
                table: "stations");

            migrationBuilder.DropTable(
                name: "AnalyticsJobRuns");

            migrationBuilder.DropTable(
                name: "FloodAnalyticsFrequency");

            migrationBuilder.DropTable(
                name: "FloodAnalyticsHotspots");

            migrationBuilder.DropTable(
                name: "FloodAnalyticsSeverity");

            migrationBuilder.DropTable(
                name: "FloodEvents");

            migrationBuilder.DropTable(
                name: "AnalyticsJobs");

            migrationBuilder.DropTable(
                name: "AdministrativeAreas");

            migrationBuilder.DropIndex(
                name: "ix_station_administrative_area",
                table: "stations");

            migrationBuilder.DropColumn(
                name: "AdministrativeAreaId",
                table: "stations");
        }
    }
}
