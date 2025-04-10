using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication5.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatesss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_AspNetUsers_CommercialId",
                table: "Visits");

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VisitChecklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    ObjectiveType = table.Column<int>(type: "int", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    Encours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReglementPret = table.Column<bool>(type: "bit", nullable: true),
                    Recupere = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitChecklists_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitChecklistId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    UnitPriceHT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitOrderItems_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitOrderItems_VisitChecklists_VisitChecklistId",
                        column: x => x.VisitChecklistId,
                        principalTable: "VisitChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisitChecklists_VisitId",
                table: "VisitChecklists",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitOrderItems_ArticleId",
                table: "VisitOrderItems",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitOrderItems_VisitChecklistId",
                table: "VisitOrderItems",
                column: "VisitChecklistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Commercials_CommercialId",
                table: "Visits",
                column: "CommercialId",
                principalTable: "Commercials",
                principalColumn: "Cref",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Commercials_CommercialId",
                table: "Visits");

            migrationBuilder.DropTable(
                name: "VisitOrderItems");

            migrationBuilder.DropTable(
                name: "VisitChecklists");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Articles");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_AspNetUsers_CommercialId",
                table: "Visits",
                column: "CommercialId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
