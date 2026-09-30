using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Data;

public class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<FinanceTransaction> Transactions => Set<FinanceTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => new { x.Name, x.Type }).IsUnique();
            entity.Property(x => x.Type).HasConversion<string>();
        });

        modelBuilder.Entity<FinanceTransaction>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
