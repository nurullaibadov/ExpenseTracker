using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Expense;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Expense>> GetByUserIdAsync(Guid userId) =>
        await _dbSet.Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

    public async Task<Expense?> GetWithCategoryAsync(Guid id, Guid userId) =>
        await _dbSet.Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

    public async Task<PagedResult<ExpenseResponseDto>> GetExpensesAsync(Guid userId, ExpenseFilterDto filter)
    {
        var query = _dbSet.Include(e => e.Category)
            .Where(e => e.UserId == userId);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(e => e.Title.Contains(filter.SearchTerm) || (e.Description != null && e.Description.Contains(filter.SearchTerm)));

        if (filter.CategoryId.HasValue) query = query.Where(e => e.CategoryId == filter.CategoryId);
        if (filter.Type.HasValue) query = query.Where(e => e.Type == filter.Type);
        if (filter.PaymentMethod.HasValue) query = query.Where(e => e.PaymentMethod == filter.PaymentMethod);
        if (filter.StartDate.HasValue) query = query.Where(e => e.Date >= filter.StartDate);
        if (filter.EndDate.HasValue) query = query.Where(e => e.Date <= filter.EndDate);
        if (filter.MinAmount.HasValue) query = query.Where(e => e.Amount >= filter.MinAmount);
        if (filter.MaxAmount.HasValue) query = query.Where(e => e.Amount <= filter.MaxAmount);

        query = filter.SortBy?.ToLower() switch
        {
            "amount" => filter.SortDescending ? query.OrderByDescending(e => e.Amount) : query.OrderBy(e => e.Amount),
            "title" => filter.SortDescending ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
            _ => filter.SortDescending ? query.OrderByDescending(e => e.Date) : query.OrderBy(e => e.Date)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return new PagedResult<ExpenseResponseDto>
        {
            Items = items.Select(e => new ExpenseResponseDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Amount = e.Amount,
                Date = e.Date,
                Type = e.Type.ToString(),
                PaymentMethod = e.PaymentMethod.ToString(),
                Notes = e.Notes,
                IsRecurring = e.IsRecurring,
                RecurringInterval = e.RecurringInterval?.ToString(),
                Tags = e.Tags,
                ReceiptUrl = e.ReceiptUrl,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name,
                CategoryColor = e.Category.Color,
                CategoryIcon = e.Category.Icon,
                CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = total,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<ExpenseSummaryDto> GetSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet.Include(e => e.Category).Where(e => e.UserId == userId);
        if (startDate.HasValue) query = query.Where(e => e.Date >= startDate);
        if (endDate.HasValue) query = query.Where(e => e.Date <= endDate);

        var all = await query.ToListAsync();
        var expenses = all.Where(e => e.Type == ExpenseType.Expense).ToList();
        var income = all.Where(e => e.Type == ExpenseType.Income).ToList();

        var totalExp = expenses.Sum(e => e.Amount);
        var categoryBreakdown = expenses
            .GroupBy(e => new { e.Category.Name, e.Category.Color })
            .Select(g => new CategorySummaryDto
            {
                CategoryName = g.Key.Name,
                CategoryColor = g.Key.Color,
                TotalAmount = g.Sum(e => e.Amount),
                Count = g.Count(),
                Percentage = totalExp > 0 ? Math.Round(g.Sum(e => e.Amount) / totalExp * 100, 2) : 0
            }).OrderByDescending(c => c.TotalAmount).ToList();

        var monthlyTrend = all
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new MonthlyTrendDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                TotalExpenses = g.Where(e => e.Type == ExpenseType.Expense).Sum(e => e.Amount),
                TotalIncome = g.Where(e => e.Type == ExpenseType.Income).Sum(e => e.Amount)
            }).OrderBy(m => m.Year).ThenBy(m => m.Month).ToList();

        return new ExpenseSummaryDto
        {
            TotalExpenses = totalExp,
            TotalIncome = income.Sum(e => e.Amount),
            Balance = income.Sum(e => e.Amount) - totalExp,
            ExpenseCount = expenses.Count,
            IncomeCount = income.Count,
            CategoryBreakdown = categoryBreakdown,
            MonthlyTrend = monthlyTrend
        };
    }
}
