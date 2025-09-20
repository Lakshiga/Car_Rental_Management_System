using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCarAllowedKmAndOdometerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowedKmPerDay",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastOdometerReading",
                table: "Cars",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 20, 14, 25, 26, 683, DateTimeKind.Local).AddTicks(8617), "$2a$11$y/e6bIAj//F8.kStjYE86usbpuw.BtYvhWGvyjzuWX9VPxJHPpVgy" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedKmPerDay",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "LastOdometerReading",
                table: "Cars");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 20, 12, 16, 22, 925, DateTimeKind.Local).AddTicks(1335), "$2a$11$K9E0AB0AL1qvjCRQfKlH5OwpnhI3AoGFmDGiTPvjw1pFyjHIvKHQ2" });
        }
    }
}
