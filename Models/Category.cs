using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название категории.")]
    [StringLength(50, ErrorMessage = "Название должно содержать не более 50 символов.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Цвет должен быть в формате #RRGGBB.")]
    public string Color { get; set; } = "#64748b";

    public TransactionType Type { get; set; }

    public string? UserId { get; set; }

    public List<FinanceTransaction> Transactions { get; set; } = [];

    public List<MonthlyBudget> Budgets { get; set; } = [];
}
