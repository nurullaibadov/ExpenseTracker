using r = ExpenseTracker.Application.Common.Result;
using AutoMapper;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.User;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Interfaces.Services;

namespace ExpenseTracker.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileDto>> GetProfileAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return Result<UserProfileDto>.NotFound();
        return Result<UserProfileDto>.Success(_mapper.Map<UserProfileDto>(user));
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return Result<UserProfileDto>.NotFound();

        _mapper.Map(dto, user);
        user.UpdatedAt = DateTime.UtcNow;
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        return Result<UserProfileDto>.Success(_mapper.Map<UserProfileDto>(user), "Profile updated.");
    }

    public async Task<Result> UploadProfilePictureAsync(Guid userId, Stream imageStream, string fileName)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return Result.NotFound();

        var ext = Path.GetExtension(fileName).ToLower();
        if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(ext))
            return Result.Failure("Invalid image format. Use JPG, PNG or GIF.");

        var uploadsPath = Path.Combine("wwwroot", "uploads", "profiles");
        Directory.CreateDirectory(uploadsPath);
        var uniqueName = $"{userId}_{DateTime.UtcNow.Ticks}{ext}";
        var filePath = Path.Combine(uploadsPath, uniqueName);

        await using var fs = File.Create(filePath);
        await imageStream.CopyToAsync(fs);

        user.ProfilePicture = $"/uploads/profiles/{uniqueName}";
        user.UpdatedAt = DateTime.UtcNow;
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        return Result.Success("Profile picture updated.");
    }

    public async Task<Result> DeleteAccountAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return Result.NotFound();

        await _uow.Users.SoftDeleteAsync(user);
        await _uow.SaveChangesAsync();

        return Result.Success("Account deleted.");
    }
}
