using FinanceTracker.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Migrations;

[DbContext(typeof(FinanceDbContext))]
[Migration("20261002210000_EnableEmailConfirmation")]
public partial class EnableEmailConfirmation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Аккаунты, созданные до появления подтверждения email, не должны потерять доступ.
        migrationBuilder.Sql("UPDATE AspNetUsers SET EmailConfirmed = 1");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
