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

    public string RetryExchangeName { get; init; } = "cashflow.retry";
    public string RetryQueueName { get; init; } = "balance.transaction.created.retry";
    public string RetryRoutingKey { get; init; } = "transaction.created.retry";

    public string DeadLetterExchangeName { get; init; } = "cashflow.dlq";
    public string DeadLetterQueueName { get; init; } = "balance.transaction.created.dlq";
    public string DeadLetterRoutingKey { get; init; } = "transaction.created.dlq";

    public ushort PrefetchCount { get; init; } = 50;
    public int RetryDelayMilliseconds { get; init; } = 10000;
    public int MaxConsumerRetries { get; init; } = 5;
}