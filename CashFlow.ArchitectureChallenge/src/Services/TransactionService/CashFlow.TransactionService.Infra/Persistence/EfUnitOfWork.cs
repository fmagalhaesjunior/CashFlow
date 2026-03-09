using CashFlow.TransactionService.Application.Abstractions.Persistence;

namespace CashFlow.TransactionService.Infra.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly TransactionDbContext _dbContext;

    public EfUnitOfWork(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}