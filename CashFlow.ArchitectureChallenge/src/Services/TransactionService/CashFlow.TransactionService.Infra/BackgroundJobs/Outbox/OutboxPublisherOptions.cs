namespace CashFlow.TransactionService.Infra.BackgroundJobs.Outbox;

public sealed class OutboxPublisherOptions
{
    public const string SectionName = "OutboxPublisher";

    public int BatchSize { get; init; } = 50;
    public int PollingIntervalSeconds { get; init; } = 5;
    public int MaxRetries { get; init; } = 5;
    public int BaseRetryDelaySeconds { get; init; } = 10;
}