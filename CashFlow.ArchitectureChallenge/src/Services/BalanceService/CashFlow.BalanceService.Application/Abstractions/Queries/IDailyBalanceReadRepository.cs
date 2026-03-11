using CashFlow.BalanceService.Application.Queries.GetDailyBalance;

namespace CashFlow.BalanceService.Application.Abstractions.Queries;

public interface IDailyBalanceReadRepository
{
    Task<GetDailyBalanceResponse?> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken);
}