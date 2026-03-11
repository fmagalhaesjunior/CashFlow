using CashFlow.BalanceService.Application.Abstractions.Queries;
using CashFlow.BalanceService.Application.Queries.GetDailyBalance;
using CashFlow.BalanceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.BalanceService.Infrastructure.Queries;

public sealed class DailyBalanceReadRepository : IDailyBalanceReadRepository
{
    private readonly BalanceDbContext _dbContext;

    public DailyBalanceReadRepository(BalanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetDailyBalanceResponse?> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        return await _dbContext.DailyBalances
            .AsNoTracking()
            .Where(x => x.Date == date)
            .Select(x => new GetDailyBalanceResponse
            {
                Date = x.Date,
                TotalCredit = x.TotalCredit,
                TotalDebit = x.TotalDebit,
                Balance = x.Balance
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}