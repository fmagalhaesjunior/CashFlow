using CashFlow.BalanceService.Application.Abstractions.Persistence;
using CashFlow.BalanceService.Domain.Entities;
using CashFlow.BalanceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.BalanceService.Infrastructure.Repositories;

public sealed class DailyBalanceRepository : IDailyBalanceRepository
{
    private readonly BalanceDbContext _dbContext;

    public DailyBalanceRepository(BalanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DailyBalance?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        return _dbContext.DailyBalances
            .FirstOrDefaultAsync(x => x.Date == date, cancellationToken);
    }

    public async Task AddAsync(DailyBalance dailyBalance, CancellationToken cancellationToken)
    {
        await _dbContext.DailyBalances.AddAsync(dailyBalance, cancellationToken);
    }
}