using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleToNotificationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "NotificationLogs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                defaultValue: "Flood Notification");

            migrationBuilder.Sql(@"
                UPDATE ""NotificationLogs"" 
                SET ""Title"" = 'Flood Notification' 
                WHERE ""Title"" IS NULL;
                ");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "NotificationLogs");

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 22, 18, 13, 34, 769, DateTimeKind.Utc).AddTicks(7298), new DateTime(2026, 1, 22, 18, 13, 34, 769, DateTimeKind.Utc).AddTicks(7298) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 22, 18, 13, 34, 769, DateTimeKind.Utc).AddTicks(7298), new DateTime(2026, 1, 22, 18, 13, 34, 769, DateTimeKind.Utc).AddTicks(7298) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 22, 18, 13, 34, 769, DateTimeKind.Utc).AddTicks(7298), new DateTime(2026, 1, 22, 18, 13, 34, 769, DateTimeKind.Utc).AddTicks(7298) });

            migrationBuilder.UpdateData(
                table: "AlertCooldownConfigs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 22, 18, 13, 34, 769, DateTimeKind.Utc).AddTicks(7298), new DateTime(2026, 1, 22, 18, 13, 34, 769, DateTimeKind.Utc).AddTicks(7298) });
        }
    }
}
