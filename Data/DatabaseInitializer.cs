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
    }
}
