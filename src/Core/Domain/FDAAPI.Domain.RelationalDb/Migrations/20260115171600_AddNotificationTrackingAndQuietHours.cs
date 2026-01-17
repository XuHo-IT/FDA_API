using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTrackingAndQuietHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "QuietHoursEnd",
                table: "UserAlertSubscriptions",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "QuietHoursStart",
                table: "UserAlertSubscriptions",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotificationAt",
                table: "Alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationCount",
                table: "Alerts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationSent",
                table: "Alerts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_alerts_notification_status",
                table: "Alerts",
                columns: new[] { "NotificationSent", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_alerts_notification_status",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "QuietHoursEnd",
                table: "UserAlertSubscriptions");

            migrationBuilder.DropColumn(
                name: "QuietHoursStart",
                table: "UserAlertSubscriptions");

            migrationBuilder.DropColumn(
                name: "LastNotificationAt",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "NotificationCount",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "NotificationSent",
                table: "Alerts");
        }
    }
}
