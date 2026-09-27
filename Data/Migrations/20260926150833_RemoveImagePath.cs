using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "VolunteerActions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "VolunteerActions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
