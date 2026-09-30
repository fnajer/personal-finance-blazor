using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FinanceDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        await db.Database.MigrateAsync();

        if (await db.Categories.AnyAsync())
        {
            return;
        }

        var categories = new[]
        {
            new Category { Name = "Зарплата", Color = "#16a34a", Type = TransactionType.Income },
            new Category { Name = "Подработка", Color = "#0d9488", Type = TransactionType.Income },
            new Category { Name = "Продукты", Color = "#f97316", Type = TransactionType.Expense },
            new Category { Name = "Транспорт", Color = "#2563eb", Type = TransactionType.Expense },
            new Category { Name = "Жильё", Color = "#7c3aed", Type = TransactionType.Expense },
            new Category { Name = "Развлечения", Color = "#db2777", Type = TransactionType.Expense }
        };

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        db.Transactions.AddRange(
            new FinanceTransaction
            {
                Description = "Зарплата",
                Amount = 45000,
                Date = today.AddDays(-8),
                CategoryId = categories[0].Id
            },
            new FinanceTransaction
            {
                Description = "Покупка продуктов",
                Amount = 1850.40m,
                Date = today.AddDays(-3),
                CategoryId = categories[2].Id
            },
            new FinanceTransaction
            {
                Description = "Проездной",
                Amount = 650,
                Date = today.AddDays(-2),
                CategoryId = categories[3].Id
            });

        await db.SaveChangesAsync();
    }
}
