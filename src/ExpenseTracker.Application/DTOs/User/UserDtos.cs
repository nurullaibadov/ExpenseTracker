using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.User;

public class UpdateProfileDto
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal MonthlyBudget { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public string? ProfilePicture { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal MonthlyBudget { get; set; }
    public DateTime CreatedAt { get; set; }
}
