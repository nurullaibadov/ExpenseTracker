using r = ExpenseTracker.Application.Common.Result;
using AutoMapper;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.DTOs.User;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Interfaces.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork uow,
        ITokenService tokenService,
        IEmailService emailService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _tokenService = tokenService;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        try
        {
            if (await _uow.Users.EmailExistsAsync(dto.Email))
                return Result<AuthResponseDto>.Failure("Email already registered.");

            var user = _mapper.Map<User>(dto);
            user.Email = dto.Email.ToLower().Trim();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.Role = UserRole.User;
            user.EmailVerificationToken = _tokenService.GenerateRandomToken();
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

            await _uow.Users.AddAsync(user);

            var defaults = await _uow.Categories.GetDefaultCategoriesAsync();
            foreach (var cat in defaults)
            {
                await _uow.Categories.AddAsync(new Category
                {
                    Name = cat.Name,
                    Icon = cat.Icon,
                    Color = cat.Color,
                    Description = cat.Description,
                    IsDefault = false,
                    UserId = user.Id
                });
            }

            await _uow.SaveChangesAsync();

            await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName, user.EmailVerificationToken);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _uow.Users.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = _mapper.Map<UserAuthDto>(user)
            }, "Registration successful.", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", dto.Email);
            return Result<AuthResponseDto>.Failure("Registration failed. Please try again.");
        }
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Failure("Invalid email or password.", 401);

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure("Account is deactivated. Contact support.", 403);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = _mapper.Map<UserAuthDto>(user)
        });
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
        if (principal == null)
            return Result<AuthResponseDto>.Failure("Invalid access token.", 401);

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Result<AuthResponseDto>.Failure("Invalid token claims.", 401);

        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null || user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.", 401);

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = _mapper.Map<UserAuthDto>(user)
        });
    }

    public async Task<r> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email);
        if (user == null)
            return r.Success("If that email is registered, a reset link has been sent.");

        user.PasswordResetToken = _tokenService.GenerateRandomToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(2);
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, user.PasswordResetToken);

        return r.Success("If that email is registered, a reset link has been sent.");
    }

    public async Task<r> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email);
        if (user == null || user.PasswordResetToken != dto.Token || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return r.Failure("Invalid or expired reset token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.RefreshToken = null;
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);

        return r.Success("Password reset successfully.");
    }

    public async Task<r> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return r.NotFound();

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return r.Failure("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.RefreshToken = null;
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);

        return r.Success("Password changed successfully.");
    }

    public async Task<r> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email);
        if (user == null || user.EmailVerificationToken != dto.Token || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            return r.Failure("Invalid or expired verification token.");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        return r.Success("Email verified successfully.");
    }

    public async Task<r> ResendEmailVerificationAsync(string email)
    {
        var user = await _uow.Users.GetByEmailAsync(email);
        if (user == null || user.IsEmailVerified)
            return r.Success("If the email exists and is unverified, a new link has been sent.");

        user.EmailVerificationToken = _tokenService.GenerateRandomToken();
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, user.EmailVerificationToken);

        return r.Success("Verification email sent.");
    }

    public async Task<r> LogoutAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return r.NotFound();

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        return r.Success("Logged out successfully.");
    }
}
