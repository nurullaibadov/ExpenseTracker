using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.Category;

public class CreateCategoryDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string Color { get; set; } = "#6366F1";
}

public class UpdateCategoryDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string Color { get; set; } = "#6366F1";
}

public class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int ExpenseCount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
