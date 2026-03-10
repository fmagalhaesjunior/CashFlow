namespace CashFlow.TransactionService.Infra.Persistence.Outbox;

public sealed class OutboxEvent
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public bool Processed { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? NextAttemptAt { get; private set; }

    private OutboxEvent()
    {
    }

    public OutboxEvent(Guid id, string eventType, string payload, DateTime createdAt)
    {
        Id = id;
        EventType = eventType;
        Payload = payload;
        CreatedAt = createdAt;
        Processed = false;
        RetryCount = 0;
    }

    public void MarkAsProcessed(DateTime processedAt)
    {
        Processed = true;
        ProcessedAt = processedAt;
        LastError = null;
        LastAttemptAt = processedAt;
        NextAttemptAt = null;
    }

    public void RegisterFailure(string error, DateTime attemptedAt, DateTime nextAttemptAt)
    {
        RetryCount++;
        LastError = error;
        LastAttemptAt = attemptedAt;
        NextAttemptAt = nextAttemptAt;
    }
}