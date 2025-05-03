using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication5.Migrations
{
    /// <inheritdoc />
    public partial class AddRecoveryAmountToChecklistRapport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedRecoveryAmount",
                table: "ChecklistRapports",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingRecoveryAmount",
                table: "ChecklistRapports",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedRecoveryAmount",
                table: "ChecklistRapports");

            migrationBuilder.DropColumn(
                name: "RemainingRecoveryAmount",
                table: "ChecklistRapports");
        }
    }
}
