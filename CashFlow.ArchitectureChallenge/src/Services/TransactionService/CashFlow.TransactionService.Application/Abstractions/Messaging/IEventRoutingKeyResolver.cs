namespace CashFlow.TransactionService.Application.Abstractions.Messaging;

public interface IEventRoutingKeyResolver
{
    string Resolve(string eventType);
}