using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken) =>
        await _dbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

    public async Task<User?> GetByPasswordResetTokenAsync(string token) =>
        await _dbSet.FirstOrDefaultAsync(u => u.PasswordResetToken == token);

    public async Task<User?> GetByEmailVerificationTokenAsync(string token) =>
        await _dbSet.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

    public async Task<bool> EmailExistsAsync(string email) =>
        await _dbSet.AnyAsync(u => u.Email == email.ToLower().Trim());
}
