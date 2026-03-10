namespace CashFlow.BalanceService.Domain.Entities;

public sealed class ProcessedEvent
{
    private ProcessedEvent()
    {
    }

    public ProcessedEvent(Guid eventId, DateTime processedAt)
    {
        EventId = eventId;
        ProcessedAt = processedAt;
    }

    public Guid EventId { get; private set; }
    public DateTime ProcessedAt { get; private set; }
}