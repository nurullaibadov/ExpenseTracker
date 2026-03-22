using r = ExpenseTracker.Application.Common.Result;
using AutoMapper;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Category;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Interfaces.Services;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CategoryResponseDto>>> GetCategoriesAsync(Guid userId)
    {
        var categories = await _uow.Categories.GetUserCategoriesAsync(userId);
        return Result<IEnumerable<CategoryResponseDto>>.Success(_mapper.Map<IEnumerable<CategoryResponseDto>>(categories));
    }

    public async Task<Result<CategoryResponseDto>> GetCategoryByIdAsync(Guid id, Guid userId)
    {
        var category = await _uow.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);
        if (category == null) return Result<CategoryResponseDto>.NotFound("Category not found.");
        return Result<CategoryResponseDto>.Success(_mapper.Map<CategoryResponseDto>(category));
    }

    public async Task<Result<CategoryResponseDto>> CreateCategoryAsync(Guid userId, CreateCategoryDto dto)
    {
        var exists = await _uow.Categories.ExistsAsync(c => c.UserId == userId && c.Name == dto.Name && !c.IsDeleted);
        if (exists) return Result<CategoryResponseDto>.Failure("Category with this name already exists.");

        var category = _mapper.Map<Category>(dto);
        category.UserId = userId;

        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();

        return Result<CategoryResponseDto>.Success(_mapper.Map<CategoryResponseDto>(category), "Category created.", 201);
    }

    public async Task<Result<CategoryResponseDto>> UpdateCategoryAsync(Guid id, Guid userId, UpdateCategoryDto dto)
    {
        var category = await _uow.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);
        if (category == null) return Result<CategoryResponseDto>.NotFound("Category not found.");

        var nameExists = await _uow.Categories.ExistsAsync(c => c.UserId == userId && c.Name == dto.Name && c.Id != id && !c.IsDeleted);
        if (nameExists) return Result<CategoryResponseDto>.Failure("Category with this name already exists.");

        _mapper.Map(dto, category);
        category.UpdatedAt = DateTime.UtcNow;
        await _uow.Categories.UpdateAsync(category);
        await _uow.SaveChangesAsync();

        return Result<CategoryResponseDto>.Success(_mapper.Map<CategoryResponseDto>(category), "Category updated.");
    }

    public async Task<r> DeleteCategoryAsync(Guid id, Guid userId)
    {
        var category = await _uow.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);
        if (category == null) return r.NotFound("Category not found.");

        var hasExpenses = await _uow.Expenses.ExistsAsync(e => e.CategoryId == id && !e.IsDeleted);
        if (hasExpenses) return r.Failure("Cannot delete category with existing expenses. Reassign expenses first.");

        await _uow.Categories.SoftDeleteAsync(category);
        await _uow.SaveChangesAsync();

        return r.Success("Category deleted.");
    }
}
