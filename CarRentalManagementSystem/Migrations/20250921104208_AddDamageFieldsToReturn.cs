using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDamageFieldsToReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DamageAmount",
                table: "Returns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DamageReason",
                table: "Returns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDamage",
                table: "Returns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 21, 19, 42, 5, 831, DateTimeKind.Local).AddTicks(6183), "$2a$11$8KE.VbW0sRXir/veZX6ClOhrvaTzsddka0GswAr.S6bBMANf.I4pu" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamageAmount",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "DamageReason",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "HasDamage",
                table: "Returns");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 20, 14, 35, 1, 704, DateTimeKind.Local).AddTicks(6256), "$2a$11$0KsdCn273CWVtSwB.LtX0uPVphZBrDgPe4hNUN03OYYi5ccjf2a2." });
        }
    }
}
