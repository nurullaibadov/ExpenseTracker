using r = ExpenseTracker.Application.Common.Result;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.DTOs.User;

namespace ExpenseTracker.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
    Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<Result> VerifyEmailAsync(VerifyEmailDto dto);
    Task<Result> ResendEmailVerificationAsync(string email);
    Task<Result> LogoutAsync(Guid userId);
}

public interface IUserService
{
    Task<Result<UserProfileDto>> GetProfileAsync(Guid userId);
    Task<Result<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<Result> UploadProfilePictureAsync(Guid userId, Stream imageStream, string fileName);
    Task<Result> DeleteAccountAsync(Guid userId);
}

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string fullName, string verificationToken);
    Task SendPasswordResetEmailAsync(string email, string fullName, string resetToken);
    Task SendEmailVerificationAsync(string email, string fullName, string verificationToken);
    Task SendPasswordChangedNotificationAsync(string email, string fullName);
    Task SendGenericEmailAsync(string email, string subject, string body);
}

public interface ITokenService
{
    string GenerateAccessToken(Domain.Entities.User user);
    string GenerateRefreshToken();
    string GenerateRandomToken();
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Guid? GetUserIdFromToken(string token);
}
