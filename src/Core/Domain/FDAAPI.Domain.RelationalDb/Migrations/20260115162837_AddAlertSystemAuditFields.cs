using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertSystemAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAlertSubscriptions_stations_StationId",
                table: "UserAlertSubscriptions");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "UserAlertSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserAlertSubscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "UserAlertSubscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "NotificationLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "RetryCount",
                table: "NotificationLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "MaxRetries",
                table: "NotificationLogs",
                type: "integer",
                nullable: false,
                defaultValue: 3,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "NotificationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "NotificationLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "NotificationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Alerts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "open",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Alerts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "info",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "AlertRules",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "warning",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "RuleType",
                table: "AlertRules",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "threshold",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "ix_user_alert_subscriptions_area",
                table: "UserAlertSubscriptions",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAlertSubscriptions_Areas_AreaId",
                table: "UserAlertSubscriptions",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAlertSubscriptions_stations_StationId",
                table: "UserAlertSubscriptions",
                column: "StationId",
                principalTable: "stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAlertSubscriptions_Areas_AreaId",
                table: "UserAlertSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAlertSubscriptions_stations_StationId",
                table: "UserAlertSubscriptions");

            migrationBuilder.DropIndex(
                name: "ix_user_alert_subscriptions_area",
                table: "UserAlertSubscriptions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserAlertSubscriptions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserAlertSubscriptions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserAlertSubscriptions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "NotificationLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "NotificationLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "pending");

            migrationBuilder.AlterColumn<int>(
                name: "RetryCount",
                table: "NotificationLogs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "MaxRetries",
                table: "NotificationLogs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Alerts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "open");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Alerts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "info");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "AlertRules",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "warning");

            migrationBuilder.AlterColumn<string>(
                name: "RuleType",
                table: "AlertRules",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "threshold");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAlertSubscriptions_stations_StationId",
                table: "UserAlertSubscriptions",
                column: "StationId",
                principalTable: "stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
