using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flightmanagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_FlightCrew_CompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Routes_RouteId",
                table: "Flights");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airports_DestinationAirportId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airports_OriginAirportId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Passengers_PassportNo",
                table: "Passengers");

            migrationBuilder.DropIndex(
                name: "IX_Flights_FlightNumber_DepartureDate",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingRef",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Aircraft_TailNumber",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "Flights");

            migrationBuilder.AlterColumn<decimal>(
                name: "Fare",
                table: "Tickets",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "PassportNo",
                table: "Passengers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "RouteId",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FlightNumber",
                table: "Flights",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Flights",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "BookingRef",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "Baggage",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "IATA",
                table: "Airports",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "TailNumber",
                table: "Aircraft",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Routes_RouteId",
                table: "Flights",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "RouteId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airports_DestinationAirportId",
                table: "Routes",
                column: "DestinationAirportId",
                principalTable: "Airports",
                principalColumn: "AirportId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airports_OriginAirportId",
                table: "Routes",
                column: "OriginAirportId",
                principalTable: "Airports",
                principalColumn: "AirportId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Routes_RouteId",
                table: "Flights");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airports_DestinationAirportId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Airports_OriginAirportId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Flights");

            migrationBuilder.AlterColumn<decimal>(
                name: "Fare",
                table: "Tickets",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "PassportNo",
                table: "Passengers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "RouteId",
                table: "Flights",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FlightNumber",
                table: "Flights",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BookingRef",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "Baggage",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "IATA",
                table: "Airports",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "TailNumber",
                table: "Aircraft",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDate",
                table: "Flights",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "CONVERT(date, [DepartureTime])",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_PassportNo",
                table: "Passengers",
                column: "PassportNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightNumber_DepartureDate",
                table: "Flights",
                columns: new[] { "FlightNumber", "DepartureDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingRef",
                table: "Bookings",
                column: "BookingRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aircraft_TailNumber",
                table: "Aircraft",
                column: "TailNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Routes_RouteId",
                table: "Flights",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airports_DestinationAirportId",
                table: "Routes",
                column: "DestinationAirportId",
                principalTable: "Airports",
                principalColumn: "AirportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Airports_OriginAirportId",
                table: "Routes",
                column: "OriginAirportId",
                principalTable: "Airports",
                principalColumn: "AirportId");
        }
    }
}
