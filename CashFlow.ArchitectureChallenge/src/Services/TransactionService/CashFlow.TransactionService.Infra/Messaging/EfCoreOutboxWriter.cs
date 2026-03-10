using CashFlow.BuildingBlocks.Contracts.Messaging;
using CashFlow.TransactionService.Application.Abstractions;
using CashFlow.TransactionService.Application.Abstractions.Messaging;
using CashFlow.TransactionService.Infra.Persistence;
using CashFlow.TransactionService.Infra.Persistence.Outbox;
using Microsoft.Extensions.Logging;

namespace CashFlow.TransactionService.Infra.Messaging;

public sealed class EfCoreOutboxWriter : IOutboxWriter
{
    private readonly TransactionDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<EfCoreOutboxWriter> _logger;

    public EfCoreOutboxWriter(
        TransactionDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        ILogger<EfCoreOutboxWriter> logger)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var outboxEvent = new OutboxEvent(
            id: message.Id,
            eventType: message.Type,
            payload: message.Payload,
            occurredOnUtc: message.OccurredOnUtc,
            createdAt: _dateTimeProvider.UtcNow,
            correlationId: message.CorrelationId);

        await _dbContext.OutboxEvents.AddAsync(outboxEvent, cancellationToken);

        _logger.LogInformation(
            "Outbox message added. EventId: {EventId}, EventType: {EventType}, CorrelationId: {CorrelationId}",
            message.Id,
            message.Type,
            message.CorrelationId);
    }
}