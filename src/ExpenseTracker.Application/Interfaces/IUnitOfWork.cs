using ExpenseTracker.Application.Interfaces.Repositories;

namespace ExpenseTracker.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IExpenseRepository Expenses { get; }
    ICategoryRepository Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
