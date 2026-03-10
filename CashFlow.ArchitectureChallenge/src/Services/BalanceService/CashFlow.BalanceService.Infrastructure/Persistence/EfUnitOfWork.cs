using CashFlow.BalanceService.Application.Abstractions.Persistence;

namespace CashFlow.BalanceService.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly BalanceDbContext _dbContext;

    public EfUnitOfWork(BalanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}