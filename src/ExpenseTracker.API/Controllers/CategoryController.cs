using ExpenseTracker.Application.DTOs.Category;
using ExpenseTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get all categories for current user</summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryService.GetCategoriesAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>Get single category by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id, CurrentUserId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new category</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(CurrentUserId, dto);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCategory), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>Update an existing category</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, CurrentUserId, dto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete a category (soft delete)</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id, CurrentUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
