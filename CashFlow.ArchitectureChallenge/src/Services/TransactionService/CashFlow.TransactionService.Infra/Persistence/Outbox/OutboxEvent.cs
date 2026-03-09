namespace CashFlow.TransactionService.Infra.Persistence.Outbox;

public sealed class OutboxEvent
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public bool Processed { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

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
    }

    public void MarkAsProcessed(DateTime processedAt)
    {
        Processed = true;
        ProcessedAt = processedAt;
    }
}