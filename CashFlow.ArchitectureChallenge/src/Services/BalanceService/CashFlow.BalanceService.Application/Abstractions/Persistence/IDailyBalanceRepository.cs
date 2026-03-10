using CashFlow.BalanceService.Domain.Entities;

namespace CashFlow.BalanceService.Application.Abstractions.Persistence;

public interface IDailyBalanceRepository
{
    Task<DailyBalance?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken);
    Task AddAsync(DailyBalance dailyBalance, CancellationToken cancellationToken);
}