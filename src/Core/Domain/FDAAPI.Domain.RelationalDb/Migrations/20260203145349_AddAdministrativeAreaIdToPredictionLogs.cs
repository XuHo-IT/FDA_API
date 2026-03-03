using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministrativeAreaIdToPredictionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AreaId",
                table: "prediction_logs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "AdministrativeAreaId",
                table: "prediction_logs",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_prediction_logs_AdministrativeAreaId",
                table: "prediction_logs",
                column: "AdministrativeAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_prediction_logs_AdministrativeAreas_AdministrativeAreaId",
                table: "prediction_logs",
                column: "AdministrativeAreaId",
                principalTable: "AdministrativeAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prediction_logs_AdministrativeAreas_AdministrativeAreaId",
                table: "prediction_logs");

            migrationBuilder.DropIndex(
                name: "IX_prediction_logs_AdministrativeAreaId",
                table: "prediction_logs");

            migrationBuilder.DropColumn(
                name: "AdministrativeAreaId",
                table: "prediction_logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "AreaId",
                table: "prediction_logs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

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
        }
    }
}
