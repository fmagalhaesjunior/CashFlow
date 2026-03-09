namespace CashFlow.TransactionService.Application.Abstractions.Messaging;

public interface IOutboxWriter
{
    Task AddAsync<T>(string eventType, T payload, CancellationToken cancellationToken);
}