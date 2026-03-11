using CashFlow.BalanceService.Application.Abstractions.Persistence;
using CashFlow.BalanceService.Application.Exceptions;
using CashFlow.BalanceService.Application.UseCases.ProcessTransactionCreated;
using CashFlow.BalanceService.Domain.Entities;
using CashFlow.BuildingBlocks.Contracts.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CashFlow.BalanceService.UnitTests.Application.UseCases.ProcessTransactionCreated;

public sealed class ProcessTransactionCreatedServiceTests
{
    private readonly Mock<IDailyBalanceRepository> _dailyBalanceRepositoryMock = new();
    private readonly Mock<IProcessedEventRepository> _processedEventRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUniqueConstraintDetector> _uniqueConstraintDetectorMock = new();
    private readonly Mock<ILogger<ProcessTransactionCreatedService>> _loggerMock = new();

    private readonly ProcessTransactionCreatedService _service;

    public ProcessTransactionCreatedServiceTests()
    {
        _service = new ProcessTransactionCreatedService(
            _dailyBalanceRepositoryMock.Object,
            _processedEventRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _uniqueConstraintDetectorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_ShouldDoNothing_WhenEventWasAlreadyProcessed()
    {
        // Arrange
        var integrationEvent = CreateEvent(type: "CREDIT");

        _processedEventRepositoryMock
            .Setup(x => x.ExistsAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _service.ProcessAsync(integrationEvent, CancellationToken.None);

        // Assert
        _dailyBalanceRepositoryMock.Verify(
            x => x.GetByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCreateDailyBalance_WhenItDoesNotExist()
    {
        // Arrange
        var integrationEvent = CreateEvent(type: "CREDIT", amount: 150m);
        DailyBalance? addedDailyBalance = null;

        _processedEventRepositoryMock
            .Setup(x => x.ExistsAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dailyBalanceRepositoryMock
            .Setup(x => x.GetByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalance?)null);

        _dailyBalanceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<DailyBalance>(), It.IsAny<CancellationToken>()))
            .Callback<DailyBalance, CancellationToken>((balance, _) => addedDailyBalance = balance)
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessAsync(integrationEvent, CancellationToken.None);

        // Assert
        addedDailyBalance.Should().NotBeNull();
        addedDailyBalance!.Date.Should().Be(new DateOnly(2026, 03, 11));
        addedDailyBalance.TotalCredit.Should().Be(150m);
        addedDailyBalance.TotalDebit.Should().Be(0m);
        addedDailyBalance.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task ProcessAsync_ShouldApplyCredit_WhenTypeIsCredit()
    {
        // Arrange
        var integrationEvent = CreateEvent(type: "CREDIT", amount: 100m);
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        _processedEventRepositoryMock
            .Setup(x => x.ExistsAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dailyBalanceRepositoryMock
            .Setup(x => x.GetByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalance);

        // Act
        await _service.ProcessAsync(integrationEvent, CancellationToken.None);

        // Assert
        dailyBalance.TotalCredit.Should().Be(100m);
        dailyBalance.TotalDebit.Should().Be(0m);
        dailyBalance.Balance.Should().Be(100m);
    }

    [Fact]
    public async Task ProcessAsync_ShouldApplyDebit_WhenTypeIsDebit()
    {
        // Arrange
        var integrationEvent = CreateEvent(type: "DEBIT", amount: 40m);
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        _processedEventRepositoryMock
            .Setup(x => x.ExistsAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dailyBalanceRepositoryMock
            .Setup(x => x.GetByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalance);

        // Act
        await _service.ProcessAsync(integrationEvent, CancellationToken.None);

        // Assert
        dailyBalance.TotalCredit.Should().Be(0m);
        dailyBalance.TotalDebit.Should().Be(40m);
        dailyBalance.Balance.Should().Be(-40m);
    }

    [Fact]
    public async Task ProcessAsync_ShouldUpdateExistingDailyBalance()
    {
        // Arrange
        var integrationEvent = CreateEvent(type: "CREDIT", amount: 50m);
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));
        dailyBalance.ApplyCredit(100m);
        dailyBalance.ApplyDebit(20m);

        _processedEventRepositoryMock
            .Setup(x => x.ExistsAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dailyBalanceRepositoryMock
            .Setup(x => x.GetByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalance);

        // Act
        await _service.ProcessAsync(integrationEvent, CancellationToken.None);

        // Assert
        dailyBalance.TotalCredit.Should().Be(150m);
        dailyBalance.TotalDebit.Should().Be(20m);
        dailyBalance.Balance.Should().Be(130m);
    }

    [Fact]
    public async Task ProcessAsync_ShouldAddProcessedEvent()
    {
        // Arrange
        var integrationEvent = CreateEvent(type: "CREDIT", amount: 100m);
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        _processedEventRepositoryMock
            .Setup(x => x.ExistsAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dailyBalanceRepositoryMock
            .Setup(x => x.GetByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalance);

        // Act
        await _service.ProcessAsync(integrationEvent, CancellationToken.None);

        // Assert
        _processedEventRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ProcessedEvent>(p => p.EventId == integrationEvent.EventId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallUnitOfWorkSaveChanges()
    {
        // Arrange
        var integrationEvent = CreateEvent(type: "CREDIT", amount: 100m);
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        _processedEventRepositoryMock
            .Setup(x => x.ExistsAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dailyBalanceRepositoryMock
            .Setup(x => x.GetByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalance);

        // Act
        await _service.ProcessAsync(integrationEvent, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldThrowInvalidOperationException_WhenTypeIsInvalid()
    {
        // Arrange
        var integrationEvent = CreateEvent(type: "UNKNOWN");

        _processedEventRepositoryMock
            .Setup(x => x.ExistsAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = () => _service.ProcessAsync(integrationEvent, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Unsupported transaction type: UNKNOWN");
    }

    [Fact]
    public async Task ProcessAsync_ShouldThrowConcurrentIdempotencyException_WhenUniqueViolationOccurs()
    {
        // Arrange
        var integrationEvent = CreateEvent(type: "CREDIT", amount: 100m);
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));
        var innerException = new Exception("unique violation simulation");

        _processedEventRepositoryMock
            .Setup(x => x.ExistsAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dailyBalanceRepositoryMock
            .Setup(x => x.GetByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalance);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(innerException);

        _uniqueConstraintDetectorMock
            .Setup(x => x.IsProcessedEventUniqueViolation(innerException))
            .Returns(true);

        // Act
        var act = () => _service.ProcessAsync(integrationEvent, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConcurrentIdempotencyException>()
            .Where(x => x.EventId == integrationEvent.EventId);
    }

    private static TransactionCreatedIntegrationEvent CreateEvent(
        string type,
        decimal amount = 100m)
    {
        return new TransactionCreatedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            TransactionId = Guid.NewGuid(),
            Amount = amount,
            Type = type,
            Timestamp = new DateTime(2026, 03, 11, 10, 00, 00, DateTimeKind.Utc),
            OccurredOnUtc = new DateTime(2026, 03, 11, 10, 00, 00, DateTimeKind.Utc)
        };
    }
}