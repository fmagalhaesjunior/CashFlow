namespace CashFlow.BalanceService.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqConsumerOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public string ExchangeName { get; init; } = "cashflow.events";
    public string QueueName { get; init; } = "balance.transaction.created";
    public string RoutingKey { get; init; } = "transaction.created";
    public ushort PrefetchCount { get; init; } = 50;
}