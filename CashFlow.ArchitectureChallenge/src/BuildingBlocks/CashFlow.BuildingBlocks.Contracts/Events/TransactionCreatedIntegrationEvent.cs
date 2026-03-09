namespace CashFlow.BuildingBlocks.Contracts.Events;

public sealed class TransactionCreatedIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid TransactionId { get; init; }
    public decimal Amount { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}