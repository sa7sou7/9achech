using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication5.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitIdToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VisitId",
                table: "Notifications",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "Notifications");
        }
    }
}
