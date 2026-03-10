namespace CashFlow.TransactionService.Application.Outbox;

public sealed class OutboxEventDto
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public int RetryCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? NextAttemptAt { get; init; }
}