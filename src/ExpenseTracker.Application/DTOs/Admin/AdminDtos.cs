namespace ExpenseTracker.Application.DTOs.Admin;

public class AdminUserListDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public int ExpenseCount { get; set; }
    public decimal TotalExpenses { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalExpenses { get; set; }
    public decimal TotalExpenseAmount { get; set; }
    public int TotalCategories { get; set; }
    public int NewUsersThisMonth { get; set; }
    public List<AdminUserListDto> RecentUsers { get; set; } = new();
    public List<AdminExpenseDto> RecentExpenses { get; set; } = new();
}

public class AdminExpenseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class UpdateUserRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class AdminFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? Role { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
