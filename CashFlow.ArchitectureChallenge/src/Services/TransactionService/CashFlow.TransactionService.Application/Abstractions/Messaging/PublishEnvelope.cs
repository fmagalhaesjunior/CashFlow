namespace CashFlow.TransactionService.Application.Abstractions.Messaging;

public sealed class PublishEnvelope
{
    public Guid EventId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string? CorrelationId { get; init; }
}