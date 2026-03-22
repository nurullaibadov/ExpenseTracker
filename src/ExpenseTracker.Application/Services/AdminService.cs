using r = ExpenseTracker.Application.Common.Result;
using AutoMapper;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Admin;
using ExpenseTracker.Application.DTOs.User;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Interfaces.Services;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AdminService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<AdminDashboardDto>> GetDashboardAsync()
    {
        var allUsers = await _uow.Users.GetAllAsync();
        var allExpenses = await _uow.Expenses.GetAllAsync();
        var allCategories = await _uow.Categories.GetAllAsync();

        var userList = allUsers.ToList();
        var expenseList = allExpenses.ToList();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var recentUsers = userList.OrderByDescending(u => u.CreatedAt).Take(5).ToList();
        var recentExpensesRaw = expenseList.OrderByDescending(e => e.CreatedAt).Take(10).ToList();

        var recentExpenseDtos = new List<AdminExpenseDto>();
        foreach (var e in recentExpensesRaw)
        {
            var user = userList.FirstOrDefault(u => u.Id == e.UserId);
            var cat = (await _uow.Categories.GetByIdAsync(e.CategoryId));
            recentExpenseDtos.Add(new AdminExpenseDto
            {
                Id = e.Id,
                Title = e.Title,
                Amount = e.Amount,
                Date = e.Date,
                Type = e.Type.ToString(),
                UserName = user?.FullName ?? "",
                UserEmail = user?.Email ?? "",
                CategoryName = cat?.Name ?? ""
            });
        }

        var dashboard = new AdminDashboardDto
        {
            TotalUsers = userList.Count,
            ActiveUsers = userList.Count(u => u.IsActive),
            TotalExpenses = expenseList.Count,
            TotalExpenseAmount = expenseList.Sum(e => e.Amount),
            TotalCategories = allCategories.Count(),
            NewUsersThisMonth = userList.Count(u => u.CreatedAt >= startOfMonth),
            RecentUsers = _mapper.Map<List<AdminUserListDto>>(recentUsers),
            RecentExpenses = recentExpenseDtos
        };

        return Result<AdminDashboardDto>.Success(dashboard);
    }

    public async Task<Result<PagedResult<AdminUserListDto>>> GetUsersAsync(AdminFilterDto filter)
    {
        var allUsers = await _uow.Users.GetAllAsync();
        var query = allUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(u =>
                u.FirstName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Role) && Enum.TryParse<UserRole>(filter.Role, out var role))
            query = query.Where(u => u.Role == role);

        var total = query.Count();
        var items = query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Result<PagedResult<AdminUserListDto>>.Success(new PagedResult<AdminUserListDto>
        {
            Items = _mapper.Map<List<AdminUserListDto>>(items),
            TotalCount = total,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    public async Task<Result<UserProfileDto>> GetUserByIdAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return Result<UserProfileDto>.NotFound();
        return Result<UserProfileDto>.Success(_mapper.Map<UserProfileDto>(user));
    }

    public async Task<r> UpdateUserRoleAsync(Guid userId, UpdateUserRoleDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return r.NotFound();
        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            return r.Failure("Invalid role. Valid values: User, Admin");
        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();
        return r.Success("Role updated.");
    }

    public async Task<r> ToggleUserActiveAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return r.NotFound();
        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();
        return r.Success($"User {(user.IsActive ? "activated" : "deactivated")}.");
    }

    public async Task<r> DeleteUserAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return r.NotFound();
        await _uow.Users.SoftDeleteAsync(user);
        await _uow.SaveChangesAsync();
        return r.Success("User deleted.");
    }

    public async Task<Result<PagedResult<AdminExpenseDto>>> GetAllExpensesAsync(AdminFilterDto filter)
    {
        var allExpenses = await _uow.Expenses.GetAllAsync();
        var allUsers = await _uow.Users.GetAllAsync();
        var allCategories = await _uow.Categories.GetAllAsync();

        var userDict = allUsers.ToDictionary(u => u.Id);
        var catDict = allCategories.ToDictionary(c => c.Id);

        var query = allExpenses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (userDict.ContainsKey(e.UserId) && userDict[e.UserId].Email.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        var total = query.Count();
        var items = query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var dtos = items.Select(e => new AdminExpenseDto
        {
            Id = e.Id,
            Title = e.Title,
            Amount = e.Amount,
            Date = e.Date,
            Type = e.Type.ToString(),
            UserName = userDict.ContainsKey(e.UserId) ? userDict[e.UserId].FullName : "",
            UserEmail = userDict.ContainsKey(e.UserId) ? userDict[e.UserId].Email : "",
            CategoryName = catDict.ContainsKey(e.CategoryId) ? catDict[e.CategoryId].Name : ""
        }).ToList();

        return Result<PagedResult<AdminExpenseDto>>.Success(new PagedResult<AdminExpenseDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }
}
