using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class addnewTKDT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BookingHistories_ShowtimeId",
                table: "BookingHistories",
                column: "ShowtimeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingHistories_Showtimes_ShowtimeId",
                table: "BookingHistories",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingHistories_Showtimes_ShowtimeId",
                table: "BookingHistories");

            migrationBuilder.DropIndex(
                name: "IX_BookingHistories_ShowtimeId",
                table: "BookingHistories");
        }
    }
}
