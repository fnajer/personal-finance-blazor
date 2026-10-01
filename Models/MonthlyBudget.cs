using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceTracker.Models;

public class MonthlyBudget
{
    public int Id { get; set; }

    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Лимит должен быть больше нуля.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Limit { get; set; }

    public DateOnly Month { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию расходов.")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public string? UserId { get; set; }
}
