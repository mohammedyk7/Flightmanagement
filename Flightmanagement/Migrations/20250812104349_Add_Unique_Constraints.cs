using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flightmanagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Unique_Constraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PassportNo",
                table: "Passengers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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
                name: "IX_Airports_IATA",
                table: "Airports",
                column: "IATA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aircraft_TailNumber",
                table: "Aircraft",
                column: "TailNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_Airports_IATA",
                table: "Airports");

            migrationBuilder.DropIndex(
                name: "IX_Aircraft_TailNumber",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "Flights");

            migrationBuilder.AlterColumn<string>(
                name: "PassportNo",
                table: "Passengers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "FlightNumber",
                table: "Flights",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "BookingRef",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "TailNumber",
                table: "Aircraft",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
