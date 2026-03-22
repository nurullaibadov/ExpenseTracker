using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class Expense : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ExpenseType Type { get; set; } = ExpenseType.Expense;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReceiptUrl { get; set; }
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; } = false;
    public RecurringInterval? RecurringInterval { get; set; }
    public string? Tags { get; set; }

    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
