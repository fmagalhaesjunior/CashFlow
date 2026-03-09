using CashFlow.TransactionService.Application.Abstractions;
using CashFlow.TransactionService.Application.Abstractions.Messaging;
using CashFlow.TransactionService.Infra.Persistence;
using CashFlow.TransactionService.Infra.Persistence.Outbox;
using System.Text.Json;

namespace CashFlow.TransactionService.Infra.Messaging;

public sealed class EfCoreOutboxWriter : IOutboxWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly TransactionDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EfCoreOutboxWriter(
        TransactionDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task AddAsync<T>(string eventType, T payload, CancellationToken cancellationToken)
    {
        var jsonPayload = JsonSerializer.Serialize(payload, SerializerOptions);

        var outboxEvent = new OutboxEvent(
            Guid.NewGuid(),
            eventType,
            jsonPayload,
            _dateTimeProvider.UtcNow);

        await _dbContext.OutboxEvents.AddAsync(outboxEvent, cancellationToken);
    }
}