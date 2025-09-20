using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequiredFieldsFromRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNo",
                table: "Customers",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNo",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 20, 9, 14, 10, 186, DateTimeKind.Local).AddTicks(9473));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 20, 9, 14, 10, 186, DateTimeKind.Local).AddTicks(9480));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 20, 9, 14, 10, 186, DateTimeKind.Local).AddTicks(8671), "$2a$11$JEiEgadtct40rD5Ognws1utCb.ukfUGHQlkatbDmCVu7LAmE8TpoC" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNo",
                table: "Customers",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNo",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 19, 1, 2, 6, 991, DateTimeKind.Local).AddTicks(1615));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 19, 1, 2, 6, 991, DateTimeKind.Local).AddTicks(1619));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 19, 1, 2, 6, 991, DateTimeKind.Local).AddTicks(754), "$2a$11$KW1vJDvSIjb0yZjpmjm8PO3bhozg98BBV4//fhH0hkja66czYYRRG" });
        }
    }
}
