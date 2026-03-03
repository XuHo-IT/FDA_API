using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeColumnToStations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "stations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "urban_lowland",
                comment: "Station type: urban_river, urban_lowland, coastal, industrial, mountain, highland");

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

            migrationBuilder.CreateIndex(
                name: "ix_station_type",
                table: "stations",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_station_type",
                table: "stations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "stations");

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 22, 18, 17, 7, 712, DateTimeKind.Utc).AddTicks(9740), new DateTime(2026, 1, 22, 18, 17, 7, 712, DateTimeKind.Utc).AddTicks(9740) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 22, 18, 17, 7, 712, DateTimeKind.Utc).AddTicks(9740), new DateTime(2026, 1, 22, 18, 17, 7, 712, DateTimeKind.Utc).AddTicks(9740) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 22, 18, 17, 7, 712, DateTimeKind.Utc).AddTicks(9740), new DateTime(2026, 1, 22, 18, 17, 7, 712, DateTimeKind.Utc).AddTicks(9740) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 22, 18, 17, 7, 712, DateTimeKind.Utc).AddTicks(9740), new DateTime(2026, 1, 22, 18, 17, 7, 712, DateTimeKind.Utc).AddTicks(9740) });
        }
    }
}
