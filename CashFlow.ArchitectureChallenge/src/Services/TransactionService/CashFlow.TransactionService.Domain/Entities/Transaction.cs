using CashFlow.BuildingBlocks.Domain.Abstractions;
using CashFlow.TransactionService.Domain.Enums;
using CashFlow.TransactionService.Domain.Events;

namespace CashFlow.TransactionService.Domain.Entities;

public sealed class Transaction : AggregateRoot
{
    private Transaction()
    {
    }

    private Transaction(Guid id, decimal amount, TransactionType type, string description, DateTime timestamp)
    {
        Id = id;
        Amount = amount;
        Type = type;
        Description = description;
        Timestamp = timestamp;
    }

    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Description { get; private set; } = string.Empty;

    public static Transaction Create(decimal amount, TransactionType type, string description, DateTime? timestamp = null)
    {
        Validate(amount, type, description);

        var entity = new Transaction(
            Guid.NewGuid(),
            amount,
            type,
            description.Trim(),
            timestamp ?? DateTime.UtcNow);

        entity.AddDomainEvent(new TransactionCreatedDomainEvent(
            entity.Id,
            entity.Amount,
            entity.Type,
            entity.Timestamp));

        return entity;
    }

    private static void Validate(decimal amount, TransactionType type, string description)
    {
        if (amount <= 0)
        {
            throw new DomainException("The amount must be greater than zero.");
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException("Invalid transaction type.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Description is required.");
        }

        if (description.Trim().Length > 200)
        {
            throw new DomainException("Description must have a maximum of 200 characters.");
        }
    }
}