using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using FinanceTracker.Data;

#nullable disable

namespace FinanceTracker.Migrations;

[DbContext(typeof(FinanceDbContext))]
[Migration("20260930160000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Color = table.Column<string>(type: "TEXT", nullable: false),
                Type = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Categories", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Description = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                CategoryId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Transactions_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Categories_Name_Type",
            table: "Categories",
            columns: new[] { "Name", "Type" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_CategoryId",
            table: "Transactions",
            column: "CategoryId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Transactions");
        migrationBuilder.DropTable(name: "Categories");
    }
}
