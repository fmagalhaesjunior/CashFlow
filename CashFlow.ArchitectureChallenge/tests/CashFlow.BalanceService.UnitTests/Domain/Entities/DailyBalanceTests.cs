using CashFlow.BalanceService.Domain.Entities;
using CashFlow.BuildingBlocks.Domain.Abstractions;
using FluentAssertions;

namespace CashFlow.BalanceService.UnitTests.Domain.Entities;

public sealed class DailyBalanceTests
{
    [Fact]
    public void Create_ShouldInitializeWithZeroValues()
    {
        // Arrange
        var date = new DateOnly(2026, 03, 11);

        // Act
        var dailyBalance = DailyBalance.Create(date);

        // Assert
        dailyBalance.Should().NotBeNull();
        dailyBalance.Date.Should().Be(date);
        dailyBalance.TotalCredit.Should().Be(0m);
        dailyBalance.TotalDebit.Should().Be(0m);
        dailyBalance.Balance.Should().Be(0m);
    }

    [Fact]
    public void ApplyCredit_ShouldIncreaseTotalCredit()
    {
        // Arrange
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        // Act
        dailyBalance.ApplyCredit(150m);

        // Assert
        dailyBalance.TotalCredit.Should().Be(150m);
        dailyBalance.TotalDebit.Should().Be(0m);
        dailyBalance.Balance.Should().Be(150m);
    }

    [Fact]
    public void ApplyCredit_ShouldRecalculateBalance_WhenCalledMultipleTimes()
    {
        // Arrange
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        // Act
        dailyBalance.ApplyCredit(100m);
        dailyBalance.ApplyCredit(50m);

        // Assert
        dailyBalance.TotalCredit.Should().Be(150m);
        dailyBalance.TotalDebit.Should().Be(0m);
        dailyBalance.Balance.Should().Be(150m);
    }

    [Fact]
    public void ApplyDebit_ShouldIncreaseTotalDebit()
    {
        // Arrange
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        // Act
        dailyBalance.ApplyDebit(40m);

        // Assert
        dailyBalance.TotalCredit.Should().Be(0m);
        dailyBalance.TotalDebit.Should().Be(40m);
        dailyBalance.Balance.Should().Be(-40m);
    }

    [Fact]
    public void ApplyDebit_ShouldRecalculateBalance_WhenCalledMultipleTimes()
    {
        // Arrange
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        // Act
        dailyBalance.ApplyDebit(20m);
        dailyBalance.ApplyDebit(30m);

        // Assert
        dailyBalance.TotalCredit.Should().Be(0m);
        dailyBalance.TotalDebit.Should().Be(50m);
        dailyBalance.Balance.Should().Be(-50m);
    }

    [Fact]
    public void ApplyCreditAndDebit_ShouldRecalculateBalanceCorrectly()
    {
        // Arrange
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        // Act
        dailyBalance.ApplyCredit(200m);
        dailyBalance.ApplyDebit(50m);
        dailyBalance.ApplyDebit(25m);

        // Assert
        dailyBalance.TotalCredit.Should().Be(200m);
        dailyBalance.TotalDebit.Should().Be(75m);
        dailyBalance.Balance.Should().Be(125m);
    }

    [Fact]
    public void ApplyCredit_ShouldThrowDomainException_WhenAmountIsZero()
    {
        // Arrange
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        // Act
        var act = () => dailyBalance.ApplyCredit(0m);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Credit amount must be greater than zero.");
    }

    [Fact]
    public void ApplyCredit_ShouldThrowDomainException_WhenAmountIsNegative()
    {
        // Arrange
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        // Act
        var act = () => dailyBalance.ApplyCredit(-10m);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Credit amount must be greater than zero.");
    }

    [Fact]
    public void ApplyDebit_ShouldThrowDomainException_WhenAmountIsZero()
    {
        // Arrange
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        // Act
        var act = () => dailyBalance.ApplyDebit(0m);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Debit amount must be greater than zero.");
    }

    [Fact]
    public void ApplyDebit_ShouldThrowDomainException_WhenAmountIsNegative()
    {
        // Arrange
        var dailyBalance = DailyBalance.Create(new DateOnly(2026, 03, 11));

        // Act
        var act = () => dailyBalance.ApplyDebit(-1m);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Debit amount must be greater than zero.");
    }
}