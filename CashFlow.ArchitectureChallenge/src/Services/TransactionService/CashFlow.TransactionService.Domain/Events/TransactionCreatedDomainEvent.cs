using CashFlow.BuildingBlocks.Domain.Abstractions;
using CashFlow.TransactionService.Domain.Enums;

namespace CashFlow.TransactionService.Domain.Events;

public sealed class TransactionCreatedDomainEvent : IDomainEvent
{
    public TransactionCreatedDomainEvent(Guid transactionId, decimal amount, TransactionType type, DateTime timestamp)
    {
        TransactionId = transactionId;
        Amount = amount;
        Type = type;
        Timestamp = timestamp;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public Guid TransactionId { get; }
    public decimal Amount { get; }
    public TransactionType Type { get; }
    public DateTime Timestamp { get; }
    public DateTime OccurredOnUtc { get; }
}