using r = ExpenseTracker.Application.Common.Result;
using AutoMapper;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Expense;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Interfaces.Services;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ExpenseService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ExpenseResponseDto>>> GetExpensesAsync(Guid userId, ExpenseFilterDto filter)
    {
        var result = await _uow.Expenses.GetExpensesAsync(userId, filter);
        return Result<PagedResult<ExpenseResponseDto>>.Success(result);
    }

    public async Task<Result<ExpenseResponseDto>> GetExpenseByIdAsync(Guid id, Guid userId)
    {
        var expense = await _uow.Expenses.GetWithCategoryAsync(id, userId);
        if (expense == null) return Result<ExpenseResponseDto>.NotFound("Expense not found.");
        return Result<ExpenseResponseDto>.Success(_mapper.Map<ExpenseResponseDto>(expense));
    }

    public async Task<Result<ExpenseResponseDto>> CreateExpenseAsync(Guid userId, CreateExpenseDto dto)
    {
        var categoryExists = await _uow.Categories.CategoryExistsForUserAsync(dto.CategoryId, userId);
        if (!categoryExists) return Result<ExpenseResponseDto>.Failure("Category not found.");

        var expense = _mapper.Map<Expense>(dto);
        expense.UserId = userId;

        await _uow.Expenses.AddAsync(expense);
        await _uow.SaveChangesAsync();

        var created = await _uow.Expenses.GetWithCategoryAsync(expense.Id, userId);
        return Result<ExpenseResponseDto>.Success(_mapper.Map<ExpenseResponseDto>(created!), "Expense created.", 201);
    }

    public async Task<Result<ExpenseResponseDto>> UpdateExpenseAsync(Guid id, Guid userId, UpdateExpenseDto dto)
    {
        var expense = await _uow.Expenses.GetWithCategoryAsync(id, userId);
        if (expense == null) return Result<ExpenseResponseDto>.NotFound("Expense not found.");

        var categoryExists = await _uow.Categories.CategoryExistsForUserAsync(dto.CategoryId, userId);
        if (!categoryExists) return Result<ExpenseResponseDto>.Failure("Category not found.");

        _mapper.Map(dto, expense);
        expense.UpdatedAt = DateTime.UtcNow;
        await _uow.Expenses.UpdateAsync(expense);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Expenses.GetWithCategoryAsync(id, userId);
        return Result<ExpenseResponseDto>.Success(_mapper.Map<ExpenseResponseDto>(updated!), "Expense updated.");
    }

    public async Task<r> DeleteExpenseAsync(Guid id, Guid userId)
    {
        var expense = await _uow.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId && !e.IsDeleted);
        if (expense == null) return Result.NotFound("Expense not found.");

        await _uow.Expenses.SoftDeleteAsync(expense);
        await _uow.SaveChangesAsync();

        return Result.Success("Expense deleted.");
    }

    public async Task<Result<ExpenseSummaryDto>> GetSummaryAsync(Guid userId, DateTime? startDate, DateTime? endDate)
    {
        var summary = await _uow.Expenses.GetSummaryAsync(userId, startDate, endDate);
        return Result<ExpenseSummaryDto>.Success(summary);
    }

    public async Task<Result<string>> UploadReceiptAsync(Guid expenseId, Guid userId, Stream fileStream, string fileName)
    {
        var expense = await _uow.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId && !e.IsDeleted);
        if (expense == null) return Result<string>.NotFound("Expense not found.");

        var ext = Path.GetExtension(fileName).ToLower();
        if (!new[] { ".jpg", ".jpeg", ".png", ".pdf" }.Contains(ext))
            return Result<string>.Failure("Invalid file. Use JPG, PNG or PDF.");

        var dir = Path.Combine("wwwroot", "uploads", "receipts");
        Directory.CreateDirectory(dir);
        var uniqueName = $"{expenseId}_{DateTime.UtcNow.Ticks}{ext}";
        var filePath = Path.Combine(dir, uniqueName);

        await using var fs = File.Create(filePath);
        await fileStream.CopyToAsync(fs);

        expense.ReceiptUrl = $"/uploads/receipts/{uniqueName}";
        expense.UpdatedAt = DateTime.UtcNow;
        await _uow.Expenses.UpdateAsync(expense);
        await _uow.SaveChangesAsync();

        return Result<string>.Success(expense.ReceiptUrl, "Receipt uploaded.");
    }
}
