using ExpenseTracker.Application.DTOs.Expense;
using ExpenseTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get paginated list of expenses with filters</summary>
    [HttpGet]
    public async Task<IActionResult> GetExpenses([FromQuery] ExpenseFilterDto filter)
    {
        var result = await _expenseService.GetExpensesAsync(CurrentUserId, filter);
        return Ok(result);
    }

    /// <summary>Get single expense by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetExpense(Guid id)
    {
        var result = await _expenseService.GetExpenseByIdAsync(id, CurrentUserId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new expense</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        var result = await _expenseService.CreateExpenseAsync(CurrentUserId, dto);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetExpense), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>Update an existing expense</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateExpense(Guid id, [FromBody] UpdateExpenseDto dto)
    {
        var result = await _expenseService.UpdateExpenseAsync(id, CurrentUserId, dto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete an expense (soft delete)</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var result = await _expenseService.DeleteExpenseAsync(id, CurrentUserId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>Get expense summary and statistics</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _expenseService.GetSummaryAsync(CurrentUserId, startDate, endDate);
        return Ok(result);
    }

    /// <summary>Upload receipt for an expense</summary>
    [HttpPost("{id:guid}/receipt")]
    public async Task<IActionResult> UploadReceipt(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
        if (file.Length > 10 * 1024 * 1024) return BadRequest("File size exceeds 10MB.");

        var result = await _expenseService.UploadReceiptAsync(id, CurrentUserId, file.OpenReadStream(), file.FileName);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
