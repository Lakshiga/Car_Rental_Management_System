using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNICFromRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
