using CashFlow.BalanceService.Domain.Entities;

namespace CashFlow.BalanceService.Application.Abstractions.Persistence;

public interface IProcessedEventRepository
{
    Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken);
    Task AddAsync(ProcessedEvent processedEvent, CancellationToken cancellationToken);
}