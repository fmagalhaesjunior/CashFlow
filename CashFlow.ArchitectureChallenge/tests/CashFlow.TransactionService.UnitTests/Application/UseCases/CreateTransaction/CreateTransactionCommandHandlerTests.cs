using CashFlow.BuildingBlocks.Contracts.Events;
using CashFlow.BuildingBlocks.Contracts.Messaging;
using CashFlow.TransactionService.Application.Abstractions;
using CashFlow.TransactionService.Application.Abstractions.Messaging;
using CashFlow.TransactionService.Application.Abstractions.Persistence;
using CashFlow.TransactionService.Application.UseCases.CreateTransaction;
using CashFlow.TransactionService.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CashFlow.TransactionService.UnitTests.Application.UseCases.CreateTransaction;

public sealed class CreateTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
    private readonly Mock<IOutboxWriter> _outboxWriterMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<ILogger<CreateTransactionCommandHandler>> _loggerMock = new();

    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _dateTimeProviderMock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 03, 11, 14, 00, 00, DateTimeKind.Utc));

        _handler = new CreateTransactionCommandHandler(
            _transactionRepositoryMock.Object,
            _outboxWriterMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddTransactionToRepository()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 150m,
            Type = 1,
            Description = "Venda do dia",
            CorrelationId = "corr-001"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Transaction>(t =>
                t.Amount == 150m &&
                t.Description == "Venda do dia"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldWriteOutboxMessage()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100m,
            Type = 1,
            Description = "Venda teste",
            CorrelationId = "corr-002"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _outboxWriterMock.Verify(
            x => x.AddAsync(
                It.Is<OutboxMessage>(m =>
                    m.Type == nameof(TransactionCreatedIntegrationEvent) &&
                    !string.IsNullOrWhiteSpace(m.Payload)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallUnitOfWorkSaveChanges()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 200m,
            Type = 2,
            Description = "Pagamento fornecedor",
            CorrelationId = "corr-003"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCreatedResponse()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 80m,
            Type = 1,
            Description = "Recebimento",
            CorrelationId = "corr-004"
        };

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.TransactionId.Should().NotBeEmpty();
        response.Message.Should().Be("Transaction created successfully.");
        response.Timestamp.Should().Be(_dateTimeProviderMock.Object.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldUseCorrelationIdInOutboxMessage()
    {
        // Arrange
        const string correlationId = "corr-abc-123";

        var command = new CreateTransactionCommand
        {
            Amount = 120m,
            Type = 1,
            Description = "Venda",
            CorrelationId = correlationId
        };

        OutboxMessage? capturedMessage = null;

        _outboxWriterMock
            .Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxMessage, CancellationToken>((message, _) => capturedMessage = message)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task Handle_ShouldSerializeTransactionCreatedIntegrationEventCorrectly()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 350m,
            Type = 1,
            Description = "Venda serializada",
            CorrelationId = "corr-serialize"
        };

        OutboxMessage? capturedMessage = null;

        _outboxWriterMock
            .Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxMessage, CancellationToken>((message, _) => capturedMessage = message)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedMessage.Should().NotBeNull();

        var integrationEvent = JsonSerializer.Deserialize<TransactionCreatedIntegrationEvent>(capturedMessage!.Payload);

        integrationEvent.Should().NotBeNull();
        integrationEvent!.TransactionId.Should().Be(response.TransactionId);
        integrationEvent.Amount.Should().Be(command.Amount);
        integrationEvent.Type.Should().Be("CREDIT");
        integrationEvent.Timestamp.Should().Be(_dateTimeProviderMock.Object.UtcNow);
        integrationEvent.EventId.Should().NotBeEmpty();
    }
}