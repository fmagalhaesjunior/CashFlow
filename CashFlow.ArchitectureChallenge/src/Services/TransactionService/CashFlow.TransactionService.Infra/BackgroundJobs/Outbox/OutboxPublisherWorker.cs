using CashFlow.TransactionService.Application.Abstractions;
using CashFlow.TransactionService.Application.Abstractions.Messaging;
using CashFlow.TransactionService.Application.Abstractions.Outbox;
using CashFlow.TransactionService.Application.Abstractions.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CashFlow.TransactionService.Infra.BackgroundJobs.Outbox;

public sealed class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly OutboxPublisherOptions _options;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        IDateTimeProvider dateTimeProvider,
        IOptions<OutboxPublisherOptions> options,
        ILogger<OutboxPublisherWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox publisher worker started. BatchSize: {BatchSize}, PollingIntervalSeconds: {PollingIntervalSeconds}, MaxRetries: {MaxRetries}",
            _options.BatchSize,
            _options.PollingIntervalSeconds,
            _options.MaxRetries);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Outbox publisher worker is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in outbox publisher worker.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(_options.PollingIntervalSeconds),
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Outbox publisher delay canceled due to shutdown.");
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var utcNow = _dateTimeProvider.UtcNow;

        var events = await outboxRepository.GetPendingAsync(
            _options.BatchSize,
            utcNow,
            cancellationToken);

        if (events.Count == 0)
        {
            _logger.LogDebug("No pending outbox events found.");
            return;
        }

        _logger.LogInformation("Processing outbox batch. Count: {Count}", events.Count);

        foreach (var outboxEvent in events)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await publisher.PublishAsync(
                    new PublishEnvelope
                    {
                        EventId = outboxEvent.Id,
                        EventType = outboxEvent.EventType,
                        Payload = outboxEvent.Payload,
                        CorrelationId = outboxEvent.CorrelationId
                    },
                    cancellationToken);

                await outboxRepository.MarkAsProcessedAsync(
                    outboxEvent.Id,
                    _dateTimeProvider.UtcNow,
                    cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Outbox event processed successfully. EventId: {EventId}, EventType: {EventType}, CorrelationId: {CorrelationId}",
                    outboxEvent.Id,
                    outboxEvent.EventType,
                    outboxEvent.CorrelationId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Outbox processing canceled. EventId: {EventId}",
                    outboxEvent.Id);

                throw;
            }
            catch (Exception ex)
            {
                var attemptedAt = _dateTimeProvider.UtcNow;
                var retryNumber = outboxEvent.RetryCount + 1;

                if (retryNumber > _options.MaxRetries)
                {
                    await outboxRepository.MarkAsDeadLetteredAsync(
                        outboxEvent.Id,
                        ex.Message,
                        attemptedAt,
                        cancellationToken);

                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogError(
                        ex,
                        "Outbox event dead-lettered. EventId: {EventId}, EventType: {EventType}, CorrelationId: {CorrelationId}, RetryCount: {RetryCount}",
                        outboxEvent.Id,
                        outboxEvent.EventType,
                        outboxEvent.CorrelationId,
                        retryNumber);

                    continue;
                }

                var nextAttemptAt = CalculateNextAttemptAt(outboxEvent.RetryCount, attemptedAt);

                await outboxRepository.RegisterFailureAsync(
                    outboxEvent.Id,
                    ex.Message,
                    attemptedAt,
                    nextAttemptAt,
                    cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    ex,
                    "Failed to publish outbox event. EventId: {EventId}, EventType: {EventType}, CorrelationId: {CorrelationId}, RetryCount: {RetryCount}, NextAttemptAt: {NextAttemptAt}",
                    outboxEvent.Id,
                    outboxEvent.EventType,
                    outboxEvent.CorrelationId,
                    retryNumber,
                    nextAttemptAt);
            }
        }
    }

    private DateTime CalculateNextAttemptAt(int currentRetryCount, DateTime attemptedAt)
    {
        var delaySeconds = _options.BaseRetryDelaySeconds * Math.Pow(2, currentRetryCount);
        return attemptedAt.AddSeconds(delaySeconds);
    }
}