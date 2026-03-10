namespace CashFlow.TransactionService.Application.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken);
}