using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddFloodHistoryAggregationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sensor_daily_agg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    station_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    max_level = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    min_level = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    avg_level = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    rainfall_total = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    reading_count = table.Column<int>(type: "integer", nullable: false),
                    flood_hours = table.Column<int>(type: "integer", nullable: false),
                    peak_severity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensor_daily_agg", x => x.id);
                    table.ForeignKey(
                        name: "FK_sensor_daily_agg_stations_station_id",
                        column: x => x.station_id,
                        principalTable: "stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sensor_hourly_agg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    station_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hour_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    max_level = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    min_level = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    avg_level = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    reading_count = table.Column<int>(type: "integer", nullable: false),
                    quality_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensor_hourly_agg", x => x.id);
                    table.ForeignKey(
                        name: "FK_sensor_hourly_agg_stations_station_id",
                        column: x => x.station_id,
                        principalTable: "stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_daily_agg_date",
                table: "sensor_daily_agg",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "uq_daily_agg_station_date",
                table: "sensor_daily_agg",
                columns: new[] { "station_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hourly_agg_hour",
                table: "sensor_hourly_agg",
                column: "hour_start");

            migrationBuilder.CreateIndex(
                name: "uq_hourly_agg_station_hour",
                table: "sensor_hourly_agg",
                columns: new[] { "station_id", "hour_start" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sensor_daily_agg");

            migrationBuilder.DropTable(
                name: "sensor_hourly_agg");
        }
    }
}
