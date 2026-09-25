using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockTrader.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemorySummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemorySummaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaperPortfolios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InitialCapital = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperPortfolios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradingMemories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingMemories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaperPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaperPortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<int>(type: "int", precision: 18, scale: 6, nullable: false),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastUpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaperPositions_PaperPortfolios_PaperPortfolioId",
                        column: x => x.PaperPortfolioId,
                        principalTable: "PaperPortfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaperTrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaperPortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExecutedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperTrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaperTrades_PaperPortfolios_PaperPortfolioId",
                        column: x => x.PaperPortfolioId,
                        principalTable: "PaperPortfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaperPositions_PaperPortfolioId_Symbol",
                table: "PaperPositions",
                columns: new[] { "PaperPortfolioId", "Symbol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaperTrades_PaperPortfolioId_ExecutedAtUtc",
                table: "PaperTrades",
                columns: new[] { "PaperPortfolioId", "ExecutedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TradingMemories_CreatedOnUtc",
                table: "TradingMemories",
                column: "CreatedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TradingMemories_Symbol",
                table: "TradingMemories",
                column: "Symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemorySummaries");

            migrationBuilder.DropTable(
                name: "PaperPositions");

            migrationBuilder.DropTable(
                name: "PaperTrades");

            migrationBuilder.DropTable(
                name: "TradingMemories");

            migrationBuilder.DropTable(
                name: "PaperPortfolios");
        }
    }
}
