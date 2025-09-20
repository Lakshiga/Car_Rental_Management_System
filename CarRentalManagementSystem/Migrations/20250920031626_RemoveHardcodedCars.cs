using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarRentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHardcodedCars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "CarID",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 20, 12, 16, 22, 925, DateTimeKind.Local).AddTicks(1335), "$2a$11$K9E0AB0AL1qvjCRQfKlH5OwpnhI3AoGFmDGiTPvjw1pFyjHIvKHQ2" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "CarID", "CarBrand", "CarModel", "CarName", "CarType", "CreatedAt", "FuelType", "ImageUrl", "IsAvailable", "Mileage", "NumberPlate", "PerKmRate", "RentPerDay", "SeatingCapacity", "Status" },
                values: new object[,]
                {
                    { 1, "Toyota", "2023", "Toyota Corolla", "Sedan", new DateTime(2025, 9, 20, 10, 37, 4, 794, DateTimeKind.Local).AddTicks(2816), "Petrol", "/images/cars/corolla.jpg", true, 15.5, "ABC-1234", 50m, 5000m, 5, "Available" },
                    { 2, "Honda", "2023", "Honda Civic", "Sedan", new DateTime(2025, 9, 20, 10, 37, 4, 794, DateTimeKind.Local).AddTicks(2822), "Petrol", "/images/cars/civic.jpg", true, 14.800000000000001, "XYZ-5678", 55m, 6000m, 5, "Available" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 20, 10, 37, 4, 794, DateTimeKind.Local).AddTicks(1975), "$2a$11$DijlXlG2lYifu2TdoboB9ul8haVTXiRrCRFsfNgNzPGVLxF0Ax5bC" });
        }
    }
}
