using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Expense;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task<User?> GetByEmailVerificationTokenAsync(string token);
    Task<bool> EmailExistsAsync(string email);
}

public interface IExpenseRepository : IGenericRepository<Expense>
{
    Task<PagedResult<ExpenseResponseDto>> GetExpensesAsync(Guid userId, ExpenseFilterDto filter);
    Task<ExpenseSummaryDto> GetSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<Expense>> GetByUserIdAsync(Guid userId);
    Task<Expense?> GetWithCategoryAsync(Guid id, Guid userId);
}

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IEnumerable<Category>> GetUserCategoriesAsync(Guid userId);
    Task<IEnumerable<Category>> GetDefaultCategoriesAsync();
    Task<bool> CategoryExistsForUserAsync(Guid categoryId, Guid userId);
}
