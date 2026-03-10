using CashFlow.TransactionService.Application.Abstractions.Messaging;

namespace CashFlow.TransactionService.Infra.Messaging.RabbitMq;

public sealed class EventRoutingKeyResolver : IEventRoutingKeyResolver
{
    private static readonly IReadOnlyDictionary<string, string> RoutingKeys =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TransactionCreatedIntegrationEvent"] = "transaction.created"
        };

    public string Resolve(string eventType)
    {
        return RoutingKeys.TryGetValue(eventType, out var routingKey) ? routingKey : "unknown.event";
    }
}