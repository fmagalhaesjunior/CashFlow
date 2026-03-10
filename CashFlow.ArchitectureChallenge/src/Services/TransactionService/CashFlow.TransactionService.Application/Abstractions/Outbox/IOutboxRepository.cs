using CashFlow.TransactionService.Application.Outbox;

namespace CashFlow.TransactionService.Application.Abstractions.Outbox;

public interface IOutboxRepository
{
    Task<IReadOnlyList<OutboxEventDto>> GetPendingAsync(
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken);

    Task MarkAsProcessedAsync(
        Guid id,
        DateTime processedAt,
        CancellationToken cancellationToken);

    Task RegisterFailureAsync(
        Guid id,
        string error,
        DateTime attemptedAt,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken);

    Task MarkAsDeadLetteredAsync(
        Guid id,
        string error,
        DateTime attemptedAt,
        CancellationToken cancellationToken);
}