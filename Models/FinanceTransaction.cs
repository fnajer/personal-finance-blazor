using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceTracker.Models;

public class FinanceTransaction
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите описание операции.")]
    [StringLength(120, ErrorMessage = "Описание должно содержать не более 120 символов.")]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Сумма должна быть больше нуля.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Укажите дату операции.")]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [StringLength(500, ErrorMessage = "Заметка должна содержать не более 500 символов.")]
    public string? Note { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию.")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}
