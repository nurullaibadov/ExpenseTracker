using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Category>> GetUserCategoriesAsync(Guid userId) =>
        await _dbSet.Include(c => c.Expenses.Where(e => !e.IsDeleted))
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<IEnumerable<Category>> GetDefaultCategoriesAsync() =>
        await _dbSet.Where(c => c.IsDefault).ToListAsync();

    public async Task<bool> CategoryExistsForUserAsync(Guid categoryId, Guid userId) =>
        await _dbSet.AnyAsync(c => c.Id == categoryId && c.UserId == userId && !c.IsDeleted);
}
