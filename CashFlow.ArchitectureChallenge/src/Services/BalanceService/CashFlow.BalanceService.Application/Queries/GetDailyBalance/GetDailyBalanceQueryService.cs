using CashFlow.BalanceService.Application.Abstractions.Queries;
using Microsoft.Extensions.Logging;

namespace CashFlow.BalanceService.Application.Queries.GetDailyBalance;

public sealed class GetDailyBalanceQueryService
{
    private readonly IDailyBalanceReadRepository _readRepository;
    private readonly ILogger<GetDailyBalanceQueryService> _logger;

    public GetDailyBalanceQueryService(
        IDailyBalanceReadRepository readRepository,
        ILogger<GetDailyBalanceQueryService> logger)
    {
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task<GetDailyBalanceResponse?> ExecuteAsync(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving daily balance. Date: {Date}",
            date);

        var result = await _readRepository.GetByDateAsync(date, cancellationToken);

        if (result is null)
        {
            _logger.LogInformation(
                "Daily balance not found. Date: {Date}",
                date);

            return null;
        }

        _logger.LogInformation(
            "Daily balance retrieved successfully. Date: {Date}, Balance: {Balance}",
            result.Date,
            result.Balance);

        return result;
    }
}