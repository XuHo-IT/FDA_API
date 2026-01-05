using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FDAAPI.Domain.RelationalDb.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Code", "Name" },
                values: new object[] { "AUTHORITY", "Authority Officer" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), "SUPERADMIN", "Super Administrator" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Code", "Name" },
                values: new object[] { "MODERATOR", "Moderator Government Officer" });
        }
    }
}






