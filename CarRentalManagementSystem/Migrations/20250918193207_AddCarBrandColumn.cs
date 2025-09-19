using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCarBrandColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarBrand",
                table: "Cars",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    ContactID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsReplied = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.ContactID);
                });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 1,
                columns: new[] { "CarBrand", "CreatedAt" },
                values: new object[] { "Toyota", new DateTime(2025, 9, 19, 1, 2, 6, 991, DateTimeKind.Local).AddTicks(1615) });

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 2,
                columns: new[] { "CarBrand", "CreatedAt" },
                values: new object[] { "Honda", new DateTime(2025, 9, 19, 1, 2, 6, 991, DateTimeKind.Local).AddTicks(1619) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 19, 1, 2, 6, 991, DateTimeKind.Local).AddTicks(754), "$2a$11$KW1vJDvSIjb0yZjpmjm8PO3bhozg98BBV4//fhH0hkja66czYYRRG" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropColumn(
                name: "CarBrand",
                table: "Cars");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 18, 19, 24, 21, 951, DateTimeKind.Local).AddTicks(3160));

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 18, 19, 24, 21, 951, DateTimeKind.Local).AddTicks(3164));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 18, 19, 24, 21, 951, DateTimeKind.Local).AddTicks(2322), "$2a$11$g3XCwFLyORzFe7tz06K6ZOUtwZ4P46rapW2.Ry9ZER4.1PRHPawqu" });
        }
    }
}
