using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnSettlementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdvancePaid",
                table: "Returns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalPaymentDate",
                table: "Returns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPaymentDue",
                table: "Returns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Returns",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 20, 14, 35, 1, 704, DateTimeKind.Local).AddTicks(6256), "$2a$11$0KsdCn273CWVtSwB.LtX0uPVphZBrDgPe4hNUN03OYYi5ccjf2a2." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvancePaid",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "FinalPaymentDate",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "FinalPaymentDue",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Returns");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 9, 20, 14, 25, 26, 683, DateTimeKind.Local).AddTicks(8617), "$2a$11$y/e6bIAj//F8.kStjYE86usbpuw.BtYvhWGvyjzuWX9VPxJHPpVgy" });
        }
    }
}
