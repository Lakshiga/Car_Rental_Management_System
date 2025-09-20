using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequirePasswordReset",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileComplete",
                table: "Staff",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "NIC",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Bookings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 20, 10, 37, 4, 794, DateTimeKind.Local).AddTicks(2816));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 20, 10, 37, 4, 794, DateTimeKind.Local).AddTicks(2822));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password", "RequirePasswordReset" },
                values: new object[] { new DateTime(2025, 9, 20, 10, 37, 4, 794, DateTimeKind.Local).AddTicks(1975), "$2a$11$DijlXlG2lYifu2TdoboB9ul8haVTXiRrCRFsfNgNzPGVLxF0Ax5bC", false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequirePasswordReset",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsProfileComplete",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "NIC",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 20, 9, 28, 7, 196, DateTimeKind.Local).AddTicks(7802));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 20, 9, 28, 7, 196, DateTimeKind.Local).AddTicks(7808));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 20, 9, 28, 7, 196, DateTimeKind.Local).AddTicks(6369), "$2a$11$CmUveD0NHFRiOGpGC6gvUex3H7PaSWu3HIGvjXiA8mH2mVWgQ8a.S" });
        }
    }
}
