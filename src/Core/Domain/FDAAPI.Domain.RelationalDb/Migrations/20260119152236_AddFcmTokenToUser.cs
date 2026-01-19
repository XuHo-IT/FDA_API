using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddFcmTokenToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                table: "Users",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AlertCooldownConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CooldownMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    MaxNotificationsPerHour = table.Column<int>(type: "integer", nullable: false, defaultValue: 6),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertCooldownConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AlertCooldownConfigs",
                columns: new[] { "Id", "CooldownMinutes", "CreatedAt", "CreatedBy", "Description", "IsActive", "MaxNotificationsPerHour", "Severity", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 30, new DateTime(2026, 1, 19, 15, 22, 33, 866, DateTimeKind.Utc).AddTicks(795), new Guid("00000000-0000-0000-0000-000000000000"), "Low priority alerts - 30 min cooldown", true, 2, "info", new DateTime(2026, 1, 19, 15, 22, 33, 866, DateTimeKind.Utc).AddTicks(795), new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 20, new DateTime(2026, 1, 19, 15, 22, 33, 866, DateTimeKind.Utc).AddTicks(795), new Guid("00000000-0000-0000-0000-000000000000"), "Caution alerts - 20 min cooldown", true, 3, "caution", new DateTime(2026, 1, 19, 15, 22, 33, 866, DateTimeKind.Utc).AddTicks(795), new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 10, new DateTime(2026, 1, 19, 15, 22, 33, 866, DateTimeKind.Utc).AddTicks(795), new Guid("00000000-0000-0000-0000-000000000000"), "Warning alerts - 10 min cooldown", true, 6, "warning", new DateTime(2026, 1, 19, 15, 22, 33, 866, DateTimeKind.Utc).AddTicks(795), new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 5, new DateTime(2026, 1, 19, 15, 22, 33, 866, DateTimeKind.Utc).AddTicks(795), new Guid("00000000-0000-0000-0000-000000000000"), "Critical alerts - 5 min cooldown", true, 12, "critical", new DateTime(2026, 1, 19, 15, 22, 33, 866, DateTimeKind.Utc).AddTicks(795), new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_FcmToken",
                table: "Users",
                column: "FcmToken");

            migrationBuilder.CreateIndex(
                name: "IX_AlertCooldownConfig_IsActive",
                table: "AlertCooldownConfigs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AlertCooldownConfig_Severity",
                table: "AlertCooldownConfigs",
                column: "Severity",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertCooldownConfigs");

            migrationBuilder.DropIndex(
                name: "IX_Users_FcmToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FcmToken",
                table: "Users");
        }
    }
}
