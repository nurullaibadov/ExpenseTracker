using r = ExpenseTracker.Application.Common.Result;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Admin;
using ExpenseTracker.Application.DTOs.Category;
using ExpenseTracker.Application.DTOs.Expense;
using ExpenseTracker.Application.DTOs.User;

namespace ExpenseTracker.Application.Interfaces.Services;

public interface IExpenseService
{
    Task<Result<PagedResult<ExpenseResponseDto>>> GetExpensesAsync(Guid userId, ExpenseFilterDto filter);
    Task<Result<ExpenseResponseDto>> GetExpenseByIdAsync(Guid id, Guid userId);
    Task<Result<ExpenseResponseDto>> CreateExpenseAsync(Guid userId, CreateExpenseDto dto);
    Task<Result<ExpenseResponseDto>> UpdateExpenseAsync(Guid id, Guid userId, UpdateExpenseDto dto);
    Task<Result> DeleteExpenseAsync(Guid id, Guid userId);
    Task<Result<ExpenseSummaryDto>> GetSummaryAsync(Guid userId, DateTime? startDate, DateTime? endDate);
    Task<Result<string>> UploadReceiptAsync(Guid expenseId, Guid userId, Stream fileStream, string fileName);
}

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryResponseDto>>> GetCategoriesAsync(Guid userId);
    Task<Result<CategoryResponseDto>> GetCategoryByIdAsync(Guid id, Guid userId);
    Task<Result<CategoryResponseDto>> CreateCategoryAsync(Guid userId, CreateCategoryDto dto);
    Task<Result<CategoryResponseDto>> UpdateCategoryAsync(Guid id, Guid userId, UpdateCategoryDto dto);
    Task<Result> DeleteCategoryAsync(Guid id, Guid userId);
}

public interface IAdminService
{
    Task<Result<AdminDashboardDto>> GetDashboardAsync();
    Task<Result<PagedResult<AdminUserListDto>>> GetUsersAsync(AdminFilterDto filter);
    Task<Result<UserProfileDto>> GetUserByIdAsync(Guid userId);
    Task<Result> UpdateUserRoleAsync(Guid userId, UpdateUserRoleDto dto);
    Task<Result> ToggleUserActiveAsync(Guid userId);
    Task<Result> DeleteUserAsync(Guid userId);
    Task<Result<PagedResult<AdminExpenseDto>>> GetAllExpensesAsync(AdminFilterDto filter);
}
