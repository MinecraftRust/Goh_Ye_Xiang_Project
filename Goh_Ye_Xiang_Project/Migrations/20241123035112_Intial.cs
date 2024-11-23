using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Goh_Ye_Xiang_Project.Migrations
{
    /// <inheritdoc />
    public partial class Intial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FacilityDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BookingDateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookingDateTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookingTimeFrom = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    BookingTimeTo = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    BookedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BookingStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");
        }
    }
}
