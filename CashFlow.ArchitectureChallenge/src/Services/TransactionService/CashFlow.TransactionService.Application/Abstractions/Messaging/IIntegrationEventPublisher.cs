namespace CashFlow.TransactionService.Application.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(PublishEnvelope envelope, CancellationToken cancellationToken);
}