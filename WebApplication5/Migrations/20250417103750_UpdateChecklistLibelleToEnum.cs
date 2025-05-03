using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication5.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChecklistLibelleToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Visits");

            migrationBuilder.RenameColumn(
                name: "VisitDate",
                table: "Visits",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Visits",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ChecklistRapports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    Libelle = table.Column<int>(type: "int", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistRapports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistRapports_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistRapports_VisitId",
                table: "ChecklistRapports",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistRapports");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Visits");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Visits",
                newName: "VisitDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Visits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
