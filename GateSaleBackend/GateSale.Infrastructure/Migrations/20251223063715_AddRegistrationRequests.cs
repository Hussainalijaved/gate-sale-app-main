using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateSale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    School = table.Column<string>(type: "text", nullable: false),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsMinor = table.Column<bool>(type: "boolean", nullable: false),
                    ParentEmail = table.Column<string>(type: "text", nullable: true),
                    ParentName = table.Column<string>(type: "text", nullable: true),
                    CognitoUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ParentalConsentGiven = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentToken = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationRequests");
        }
    }
}
