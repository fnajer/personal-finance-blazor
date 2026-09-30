using FinanceTracker.Models;

namespace FinanceTracker.Data;

public static class UserDataSeeder
{
    public static Category[] CreateDefaultCategories(string userId) =>
    [
        new() { UserId = userId, Name = "Зарплата", Color = "#16a34a", Type = TransactionType.Income },
        new() { UserId = userId, Name = "Подработка", Color = "#0d9488", Type = TransactionType.Income },
        new() { UserId = userId, Name = "Продукты", Color = "#f97316", Type = TransactionType.Expense },
        new() { UserId = userId, Name = "Транспорт", Color = "#2563eb", Type = TransactionType.Expense },
        new() { UserId = userId, Name = "Жильё", Color = "#7c3aed", Type = TransactionType.Expense },
        new() { UserId = userId, Name = "Развлечения", Color = "#db2777", Type = TransactionType.Expense }
    ];
}
