using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExposureFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountManager",
                table: "Deals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppNumber",
                table: "Deals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppStatus",
                table: "Deals",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BookingDate",
                table: "Deals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerLegalName",
                table: "Deals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysPastDue",
                table: "Deals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DealFormat",
                table: "Deals",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EquipmentCost",
                table: "Deals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinalPaymentDate",
                table: "Deals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossContract",
                table: "Deals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Deals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastNsfDate",
                table: "Deals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lessor",
                table: "Deals",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPayment",
                table: "Deals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetInvest",
                table: "Deals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "NsfCount",
                table: "Deals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Past1",
                table: "Deals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Past31",
                table: "Deals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Past61",
                table: "Deals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Past91",
                table: "Deals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentsMade",
                table: "Deals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryEquipmentCategory",
                table: "Deals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryVendor",
                table: "Deals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemainingPayments",
                table: "Deals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CustomerLegalName",
                table: "Deals",
                column: "CustomerLegalName");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_IsActive",
                table: "Deals",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_PrimaryVendor",
                table: "Deals",
                column: "PrimaryVendor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deals_CustomerLegalName",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_IsActive",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_PrimaryVendor",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "AccountManager",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "AppNumber",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "AppStatus",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "CustomerLegalName",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "DaysPastDue",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "DealFormat",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "EquipmentCost",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "FinalPaymentDate",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "GrossContract",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "LastNsfDate",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Lessor",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "MonthlyPayment",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "NetInvest",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "NsfCount",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Past1",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Past31",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Past61",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Past91",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "PaymentsMade",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "PrimaryEquipmentCategory",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "PrimaryVendor",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "RemainingPayments",
                table: "Deals");
        }
    }
}
