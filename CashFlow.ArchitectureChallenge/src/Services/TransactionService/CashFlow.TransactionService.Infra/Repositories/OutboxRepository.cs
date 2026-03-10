using CashFlow.TransactionService.Application.Abstractions.Outbox;
using CashFlow.TransactionService.Application.Outbox;
using CashFlow.TransactionService.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.TransactionService.Infra.Repositories;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly TransactionDbContext _dbContext;

    public OutboxRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OutboxEventDto>> GetPendingAsync(
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return await _dbContext.OutboxEvents
            .Where(x =>
                !x.Processed &&
                (x.NextAttemptAt == null || x.NextAttemptAt <= utcNow))
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .Select(x => new OutboxEventDto
            {
                Id = x.Id,
                EventType = x.EventType,
                Payload = x.Payload,
                RetryCount = x.RetryCount,
                CreatedAt = x.CreatedAt,
                NextAttemptAt = x.NextAttemptAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(
        Guid id,
        DateTime processedAt,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.OutboxEvents
            .FirstAsync(x => x.Id == id, cancellationToken);

        entity.MarkAsProcessed(processedAt);
    }

    public async Task RegisterFailureAsync(
        Guid id,
        string error,
        DateTime attemptedAt,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.OutboxEvents
            .FirstAsync(x => x.Id == id, cancellationToken);

        entity.RegisterFailure(error, attemptedAt, nextAttemptAt);
    }
}