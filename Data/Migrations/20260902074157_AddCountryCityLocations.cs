using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VolunteerHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryCityLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "VolunteerActions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "VolunteerActions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "VolunteerActions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VolunteerActions",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ContactInfo",
                table: "VolunteerActions",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "VolunteerActions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "VolunteerActions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Skills",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "GR", "Greece" },
                    { 2, "CH", "Switzerland" },
                    { 3, "DE", "Germany" },
                    { 4, "AT", "Austria" },
                    { 5, "FR", "France" },
                    { 6, "IT", "Italy" },
                    { 7, "ES", "Spain" },
                    { 8, "NL", "Netherlands" },
                    { 9, "BE", "Belgium" },
                    { 10, "GB", "United Kingdom" }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Athens" },
                    { 2, 1, "Thessaloniki" },
                    { 3, 1, "Patras" },
                    { 4, 1, "Heraklion" },
                    { 5, 1, "Larissa" },
                    { 6, 1, "Volos" },
                    { 7, 1, "Ioannina" },
                    { 8, 1, "Kavala" },
                    { 9, 1, "Kalamata" },
                    { 10, 1, "Serres" },
                    { 11, 2, "Zurich" },
                    { 12, 2, "Geneva" },
                    { 13, 2, "Basel" },
                    { 14, 2, "Bern" },
                    { 15, 2, "Lausanne" },
                    { 16, 2, "Lucerne" },
                    { 17, 2, "Winterthur" },
                    { 18, 2, "St. Gallen" },
                    { 19, 2, "Zug" },
                    { 20, 2, "Lugano" },
                    { 21, 3, "Berlin" },
                    { 22, 3, "Hamburg" },
                    { 23, 3, "Munich" },
                    { 24, 3, "Cologne" },
                    { 25, 3, "Frankfurt" },
                    { 26, 3, "Stuttgart" },
                    { 27, 4, "Vienna" },
                    { 28, 4, "Graz" },
                    { 29, 4, "Linz" },
                    { 30, 4, "Salzburg" },
                    { 31, 4, "Innsbruck" },
                    { 32, 5, "Paris" },
                    { 33, 5, "Marseille" },
                    { 34, 5, "Lyon" },
                    { 35, 5, "Toulouse" },
                    { 36, 5, "Nice" },
                    { 37, 6, "Rome" },
                    { 38, 6, "Milan" },
                    { 39, 6, "Naples" },
                    { 40, 6, "Turin" },
                    { 41, 6, "Florence" },
                    { 42, 7, "Madrid" },
                    { 43, 7, "Barcelona" },
                    { 44, 7, "Valencia" },
                    { 45, 7, "Seville" },
                    { 46, 7, "Malaga" },
                    { 47, 8, "Amsterdam" },
                    { 48, 8, "Rotterdam" },
                    { 49, 8, "The Hague" },
                    { 50, 8, "Utrecht" },
                    { 51, 9, "Brussels" },
                    { 52, 9, "Antwerp" },
                    { 53, 9, "Ghent" },
                    { 54, 9, "Bruges" },
                    { 55, 10, "London" },
                    { 56, 10, "Manchester" },
                    { 57, 10, "Birmingham" },
                    { 58, 10, "Liverpool" },
                    { 59, 10, "Edinburgh" },
                    { 60, 10, "Glasgow" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerActions_CityId",
                table: "VolunteerActions",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CityId",
                table: "AspNetUsers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId_Name",
                table: "Cities",
                columns: new[] { "CountryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerActions_Cities_CityId",
                table: "VolunteerActions",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerActions_Cities_CityId",
                table: "VolunteerActions");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_VolunteerActions_CityId",
                table: "VolunteerActions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CityId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "VolunteerActions");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "VolunteerActions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "VolunteerActions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Instructions",
                table: "VolunteerActions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "VolunteerActions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000);

            migrationBuilder.AlterColumn<string>(
                name: "ContactInfo",
                table: "VolunteerActions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "VolunteerActions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Skills",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
