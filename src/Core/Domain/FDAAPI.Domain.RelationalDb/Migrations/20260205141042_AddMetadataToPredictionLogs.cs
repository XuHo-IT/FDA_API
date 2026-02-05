using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataToPredictionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "prediction_logs",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS ix_prediction_logs_metadata_gin ON prediction_logs USING GIN ((\"Metadata\"));");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS ix_prediction_logs_metadata_gin;");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "prediction_logs");

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 3, 14, 53, 48, 122, DateTimeKind.Utc).AddTicks(6450), new DateTime(2026, 2, 3, 14, 53, 48, 122, DateTimeKind.Utc).AddTicks(6450) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 3, 14, 53, 48, 122, DateTimeKind.Utc).AddTicks(6450), new DateTime(2026, 2, 3, 14, 53, 48, 122, DateTimeKind.Utc).AddTicks(6450) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 3, 14, 53, 48, 122, DateTimeKind.Utc).AddTicks(6450), new DateTime(2026, 2, 3, 14, 53, 48, 122, DateTimeKind.Utc).AddTicks(6450) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 3, 14, 53, 48, 122, DateTimeKind.Utc).AddTicks(6450), new DateTime(2026, 2, 3, 14, 53, 48, 122, DateTimeKind.Utc).AddTicks(6450) });
        }
    }
}
