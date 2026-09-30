using FinanceTracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace FinanceTracker.Migrations;

[DbContext(typeof(FinanceDbContext))]
partial class FinanceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.12");

        modelBuilder.Entity("FinanceTracker.Models.Category", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            entity.Property<string>("Color").IsRequired().HasColumnType("TEXT");
            entity.Property<string>("Name").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
            entity.Property<string>("Type").IsRequired().HasColumnType("TEXT");
            entity.HasKey("Id");
            entity.HasIndex("Name", "Type").IsUnique();
            entity.ToTable("Categories");
        });

        modelBuilder.Entity("FinanceTracker.Models.FinanceTransaction", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            entity.Property<decimal>("Amount").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            entity.Property<int>("CategoryId").HasColumnType("INTEGER");
            entity.Property<DateOnly>("Date").HasColumnType("TEXT");
            entity.Property<string>("Description").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            entity.Property<string>("Note").HasMaxLength(500).HasColumnType("TEXT");
            entity.HasKey("Id");
            entity.HasIndex("CategoryId");
            entity.ToTable("Transactions");
        });

        modelBuilder.Entity("FinanceTracker.Models.FinanceTransaction", entity =>
        {
            entity.HasOne("FinanceTracker.Models.Category", "Category")
                .WithMany("Transactions")
                .HasForeignKey("CategoryId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            entity.Navigation("Category");
        });

        modelBuilder.Entity("FinanceTracker.Models.Category", entity =>
        {
            entity.Navigation("Transactions");
        });
    }
}
