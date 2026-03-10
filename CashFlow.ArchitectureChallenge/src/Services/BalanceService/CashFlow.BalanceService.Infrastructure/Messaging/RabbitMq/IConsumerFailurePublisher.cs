namespace CashFlow.BalanceService.Infrastructure.Messaging.RabbitMq;

public interface IConsumerFailurePublisher
{
    Task PublishToRetryAsync(
        string payload,
        string eventType,
        string? eventId,
        string? correlationId,
        int retryCount,
        CancellationToken cancellationToken);

    Task PublishToDeadLetterAsync(
        string payload,
        string eventType,
        string? eventId,
        string? correlationId,
        int retryCount,
        string reason,
        CancellationToken cancellationToken);
}