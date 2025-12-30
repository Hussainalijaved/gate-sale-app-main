using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateSale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RequesterEmail = table.Column<string>(type: "text", nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolRequests");
        }
    }
}
