using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Row",
                table: "Seat");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Row",
                table: "Seat",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
