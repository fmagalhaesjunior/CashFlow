using CashFlow.TransactionService.Application.UseCases.CreateTransaction;
using FluentAssertions;

namespace CashFlow.TransactionService.UnitTests.Application.UseCases.CreateTransaction;

public sealed class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
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
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAmountIsZero()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 0m,
            Type = 1,
            Description = "Venda"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateTransactionCommand.Amount));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAmountIsNegative()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = -10m,
            Type = 1,
            Description = "Venda"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateTransactionCommand.Amount));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTypeIsInvalid()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100m,
            Type = 99,
            Description = "Venda"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateTransactionCommand.Type));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenDescriptionIsEmpty()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100m,
            Type = 1,
            Description = string.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateTransactionCommand.Description));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100m,
            Type = 1,
            Description = new string('A', 201)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateTransactionCommand.Description));
    }
}