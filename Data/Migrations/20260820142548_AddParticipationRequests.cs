using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerActions_AspNetUsers_OrganizerId",
                table: "VolunteerActions");

            migrationBuilder.CreateTable(
                name: "ParticipationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolunteerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizerNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attended = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipationRequests_AspNetUsers_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipationRequests_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipationRequests_ShiftId",
                table: "ParticipationRequests",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipationRequests_VolunteerId",
                table: "ParticipationRequests",
                column: "VolunteerId");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerActions_AspNetUsers_OrganizerId",
                table: "VolunteerActions",
                column: "OrganizerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerActions_AspNetUsers_OrganizerId",
                table: "VolunteerActions");

            migrationBuilder.DropTable(
                name: "ParticipationRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerActions_AspNetUsers_OrganizerId",
                table: "VolunteerActions",
                column: "OrganizerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
