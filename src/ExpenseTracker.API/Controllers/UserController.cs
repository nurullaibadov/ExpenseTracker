using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.DTOs.User;
using ExpenseTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get current user profile</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfileAsync(CurrentUserId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>Update current user profile</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var result = await _userService.UpdateProfileAsync(CurrentUserId, dto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Upload profile picture</summary>
    [HttpPost("profile/picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
        if (file.Length > 5 * 1024 * 1024) return BadRequest("File size exceeds 5MB.");

        var result = await _userService.UploadProfilePictureAsync(CurrentUserId, file.OpenReadStream(), file.FileName);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete own account (soft delete)</summary>
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var result = await _userService.DeleteAccountAsync(CurrentUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
