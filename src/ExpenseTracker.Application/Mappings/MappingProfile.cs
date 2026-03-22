using AutoMapper;
using ExpenseTracker.Application.DTOs.Admin;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.DTOs.Category;
using ExpenseTracker.Application.DTOs.Expense;
using ExpenseTracker.Application.DTOs.User;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<User, UserAuthDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<User, AdminUserListDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.ExpenseCount, o => o.MapFrom(s => s.Expenses.Count(e => !e.IsDeleted)))
            .ForMember(d => d.TotalExpenses, o => o.MapFrom(s => s.Expenses.Where(e => !e.IsDeleted).Sum(e => e.Amount)));

        CreateMap<RegisterDto, User>()
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<UpdateProfileDto, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore());

        // Category
        CreateMap<Category, CategoryResponseDto>()
            .ForMember(d => d.ExpenseCount, o => o.MapFrom(s => s.Expenses.Count(e => !e.IsDeleted)))
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.Expenses.Where(e => !e.IsDeleted).Sum(e => e.Amount)));

        CreateMap<CreateCategoryDto, Category>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore());

        // Expense
        CreateMap<Expense, ExpenseResponseDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod.ToString()))
            .ForMember(d => d.RecurringInterval, o => o.MapFrom(s => s.RecurringInterval.HasValue ? s.RecurringInterval.ToString() : null))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.CategoryColor, o => o.MapFrom(s => s.Category.Color))
            .ForMember(d => d.CategoryIcon, o => o.MapFrom(s => s.Category.Icon));

        CreateMap<Expense, AdminExpenseDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.FullName : ""))
            .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User != null ? s.User.Email : ""))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : ""));

        CreateMap<CreateExpenseDto, Expense>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<UpdateExpenseDto, Expense>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore());
    }
}
