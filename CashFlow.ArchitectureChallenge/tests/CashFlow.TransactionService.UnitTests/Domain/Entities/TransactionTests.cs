using CashFlow.BuildingBlocks.Domain.Abstractions;
using CashFlow.TransactionService.Domain.Entities;
using CashFlow.TransactionService.Domain.Enums;
using CashFlow.TransactionService.Domain.Events;
using FluentAssertions;

namespace CashFlow.TransactionService.UnitTests.Domain.Entities;

public sealed class TransactionTests
{
    [Fact]
    public void Create_ShouldCreateTransaction_WhenDataIsValid()
    {
        // Arrange
        const decimal amount = 150.75m;
        const string description = "Venda do dia";
        var timestamp = new DateTime(2026, 03, 11, 12, 30, 00, DateTimeKind.Utc);

        // Act
        var transaction = Transaction.Create(
            amount,
            TransactionType.Credit,
            description,
            timestamp);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Id.Should().NotBeEmpty();
        transaction.Amount.Should().Be(amount);
        transaction.Type.Should().Be(TransactionType.Credit);
        transaction.Description.Should().Be(description);
        transaction.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Create_ShouldSetTimestamp_WhenTimestampIsNotProvided()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var transaction = Transaction.Create(
            100m,
            TransactionType.Debit,
            "Pagamento fornecedor");

        var after = DateTime.UtcNow;

        // Assert
        transaction.Timestamp.Should().BeOnOrAfter(before);
        transaction.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Create_ShouldTrimDescription()
    {
        // Arrange
        const string description = "   Venda com espaços   ";

        // Act
        var transaction = Transaction.Create(
            200m,
            TransactionType.Credit,
            description);

        // Assert
        transaction.Description.Should().Be("Venda com espaços");
    }

    [Fact]
    public void Create_ShouldAddDomainEvent_WhenCreated()
    {
        // Act
        var transaction = Transaction.Create(
            120m,
            TransactionType.Credit,
            "Venda teste");

        // Assert
        transaction.DomainEvents.Should().HaveCount(1);

        var domainEvent = transaction.DomainEvents.Single();
        domainEvent.Should().BeOfType<TransactionCreatedDomainEvent>();

        var typedEvent = (TransactionCreatedDomainEvent)domainEvent;
        typedEvent.TransactionId.Should().Be(transaction.Id);
        typedEvent.Amount.Should().Be(transaction.Amount);
        typedEvent.Type.Should().Be(transaction.Type);
        typedEvent.Timestamp.Should().Be(transaction.Timestamp);
        typedEvent.OccurredOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenAmountIsZero()
    {
        // Act
        var act = () => Transaction.Create(
            0m,
            TransactionType.Credit,
            "Venda inválida");

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("The amount must be greater than zero.");
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenAmountIsNegative()
    {
        // Act
        var act = () => Transaction.Create(
            -10m,
            TransactionType.Debit,
            "Pagamento inválido");

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("The amount must be greater than zero.");
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenTypeIsInvalid()
    {
        // Act
        var act = () => Transaction.Create(
            100m,
            (TransactionType)99,
            "Tipo inválido");

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Invalid transaction type.");
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenDescriptionIsEmpty()
    {
        // Act
        var act = () => Transaction.Create(
            100m,
            TransactionType.Credit,
            string.Empty);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Description is required.");
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenDescriptionIsWhiteSpace()
    {
        // Act
        var act = () => Transaction.Create(
            100m,
            TransactionType.Credit,
            "   ");

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Description is required.");
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var longDescription = new string('A', 201);

        // Act
        var act = () => Transaction.Create(
            100m,
            TransactionType.Credit,
            longDescription);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Description must have a maximum of 200 characters.");
    }
}