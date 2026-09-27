using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Countries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "TimeZoneId",
                value: "Europe/Athens");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 2,
                column: "TimeZoneId",
                value: "Europe/Zurich");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 3,
                column: "TimeZoneId",
                value: "Europe/Berlin");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 4,
                column: "TimeZoneId",
                value: "Europe/Vienna");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 5,
                column: "TimeZoneId",
                value: "Europe/Paris");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 6,
                column: "TimeZoneId",
                value: "Europe/Rome");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 7,
                column: "TimeZoneId",
                value: "Europe/Madrid");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 8,
                column: "TimeZoneId",
                value: "Europe/Amsterdam");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 9,
                column: "TimeZoneId",
                value: "Europe/Brussels");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 10,
                column: "TimeZoneId",
                value: "Europe/London");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Countries");
        }
    }
}
