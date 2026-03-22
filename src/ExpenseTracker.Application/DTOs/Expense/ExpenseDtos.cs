using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Expense;

public class CreateExpenseDto
{
    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required, Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ExpenseType Type { get; set; } = ExpenseType.Expense;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    [Required] public Guid CategoryId { get; set; }
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; } = false;
    public RecurringInterval? RecurringInterval { get; set; }
    public string? Tags { get; set; }
}

public class UpdateExpenseDto
{
    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required, Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public ExpenseType Type { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    [Required] public Guid CategoryId { get; set; }
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; }
    public RecurringInterval? RecurringInterval { get; set; }
    public string? Tags { get; set; }
}

public class ExpenseResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurringInterval { get; set; }
    public string? Tags { get; set; }
    public string? ReceiptUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExpenseFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public ExpenseType? Type { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? SortBy { get; set; } = "Date";
    public bool SortDescending { get; set; } = true;
}

public class ExpenseSummaryDto
{
    public decimal TotalExpenses { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal Balance { get; set; }
    public int ExpenseCount { get; set; }
    public int IncomeCount { get; set; }
    public List<CategorySummaryDto> CategoryBreakdown { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrend { get; set; } = new();
}

public class CategorySummaryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class MonthlyTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalExpenses { get; set; }
    public decimal TotalIncome { get; set; }
}
