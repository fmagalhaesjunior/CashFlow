namespace CashFlow.TransactionService.Infra.Persistence.Outbox;

public sealed class OutboxEvent
{
    private OutboxEvent()
    {
    }

    public OutboxEvent(
        Guid id,
        string eventType,
        string payload,
        DateTime occurredOnUtc,
        DateTime createdAt,
        string? correlationId)
    {
        Id = id;
        EventType = eventType;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
        CreatedAt = createdAt;
        CorrelationId = correlationId;
        Status = OutboxStatus.Pending;
        RetryCount = 0;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CorrelationId { get; private set; }

    public OutboxStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? NextAttemptAt { get; private set; }

    public bool IsPending => Status == OutboxStatus.Pending;
    public bool IsDeadLettered => Status == OutboxStatus.DeadLettered;

    public void MarkAsProcessed(DateTime processedAt)
    {
        Status = OutboxStatus.Processed;
        ProcessedAt = processedAt;
        LastError = null;
        LastAttemptAt = processedAt;
        NextAttemptAt = null;
    }

    public void RegisterFailure(string error, DateTime attemptedAt, DateTime nextAttemptAt)
    {
        Status = OutboxStatus.Failed;
        RetryCount++;
        LastError = error;
        LastAttemptAt = attemptedAt;
        NextAttemptAt = nextAttemptAt;
    }

    public void Requeue(DateTime nextAttemptAt)
    {
        Status = OutboxStatus.Pending;
        NextAttemptAt = nextAttemptAt;
    }

    public void MarkAsDeadLettered(string error, DateTime attemptedAt)
    {
        Status = OutboxStatus.DeadLettered;
        RetryCount++;
        LastError = error;
        LastAttemptAt = attemptedAt;
        NextAttemptAt = null;
    }
}