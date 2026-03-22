using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            if (!await context.Categories.IgnoreQueryFilters().AnyAsync(c => c.IsDefault))
            {
                var defaults = new List<Category>
                {
                    new() { Name = "Food & Dining",    Icon = "🍔", Color = "#EF4444", IsDefault = true },
                    new() { Name = "Transportation",   Icon = "🚗", Color = "#F59E0B", IsDefault = true },
                    new() { Name = "Shopping",         Icon = "🛍️", Color = "#8B5CF6", IsDefault = true },
                    new() { Name = "Entertainment",    Icon = "🎬", Color = "#EC4899", IsDefault = true },
                    new() { Name = "Healthcare",       Icon = "🏥", Color = "#10B981", IsDefault = true },
                    new() { Name = "Utilities",        Icon = "💡", Color = "#3B82F6", IsDefault = true },
                    new() { Name = "Education",        Icon = "📚", Color = "#6366F1", IsDefault = true },
                    new() { Name = "Travel",           Icon = "✈️", Color = "#14B8A6", IsDefault = true },
                    new() { Name = "Housing",          Icon = "🏠", Color = "#F97316", IsDefault = true },
                    new() { Name = "Salary",           Icon = "💰", Color = "#22C55E", IsDefault = true },
                    new() { Name = "Other",            Icon = "📦", Color = "#6B7280", IsDefault = true }
                };
                await context.Categories.AddRangeAsync(defaults);
            }

            if (!await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Role == UserRole.Admin))
            {
                var admin = new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@expensetracker.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
                    Role = UserRole.Admin,
                    IsEmailVerified = true,
                    IsActive = true
                };
                await context.Users.AddAsync(admin);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database.");
        }
    }
}
