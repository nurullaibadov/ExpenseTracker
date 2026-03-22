using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string Color { get; set; } = "#6366F1";
    public bool IsDefault { get; set; } = false;
    public Guid? UserId { get; set; }

    // Navigation
    public User? User { get; set; }
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
