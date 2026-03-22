using ExpenseTracker.Application.DTOs.Admin;
using ExpenseTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>Get admin dashboard stats</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardAsync();
        return Ok(result);
    }

    /// <summary>Get all users with pagination and filters</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] AdminFilterDto filter)
    {
        var result = await _adminService.GetUsersAsync(filter);
        return Ok(result);
    }

    /// <summary>Get a specific user by ID</summary>
    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var result = await _adminService.GetUserByIdAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>Update a user's role</summary>
    [HttpPatch("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto dto)
    {
        var result = await _adminService.UpdateUserRoleAsync(id, dto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Toggle user active/inactive status</summary>
    [HttpPatch("users/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleUserActive(Guid id)
    {
        var result = await _adminService.ToggleUserActiveAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete a user (soft delete)</summary>
    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _adminService.DeleteUserAsync(id);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>Get all expenses across all users</summary>
    [HttpGet("expenses")]
    public async Task<IActionResult> GetAllExpenses([FromQuery] AdminFilterDto filter)
    {
        var result = await _adminService.GetAllExpensesAsync(filter);
        return Ok(result);
    }
}
