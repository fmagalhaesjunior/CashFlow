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
        _logger.LogInformation("Outbox publisher worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in outbox publisher worker.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_options.PollingIntervalSeconds),
                stoppingToken);
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
            return;
        }

        foreach (var outboxEvent in events)
        {
            try
            {
                await publisher.PublishAsync(
                    outboxEvent.EventType,
                    outboxEvent.Payload,
                    cancellationToken);

                await outboxRepository.MarkAsProcessedAsync(
                    outboxEvent.Id,
                    _dateTimeProvider.UtcNow,
                    cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Outbox event processed successfully. EventId: {EventId}",
                    outboxEvent.Id);
            }
            catch (Exception ex)
            {
                var attemptedAt = _dateTimeProvider.UtcNow;
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
                    "Failed to publish outbox event. EventId: {EventId}, RetryCount: {RetryCount}, NextAttemptAt: {NextAttemptAt}",
                    outboxEvent.Id,
                    outboxEvent.RetryCount + 1,
                    nextAttemptAt);
            }
        }
    }

    private DateTime CalculateNextAttemptAt(int currentRetryCount, DateTime attemptedAt)
    {
        var retryNumber = currentRetryCount + 1;

        if (retryNumber > _options.MaxRetries)
        {
            return attemptedAt.AddYears(100);
        }

        var delaySeconds = _options.BaseRetryDelaySeconds * Math.Pow(2, currentRetryCount);

        return attemptedAt.AddSeconds(delaySeconds);
    }
}