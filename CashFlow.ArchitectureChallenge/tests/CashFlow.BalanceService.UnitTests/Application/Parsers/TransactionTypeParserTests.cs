using CashFlow.BalanceService.Application.Models;
using CashFlow.BalanceService.Application.Parsers;
using FluentAssertions;

namespace CashFlow.BalanceService.UnitTests.Application.Parsers;

public sealed class TransactionTypeParserTests
{
    [Fact]
    public void Parse_ShouldReturnCredit_WhenTypeIsCredit()
    {
        // Act
        var result = TransactionTypeParser.Parse("CREDIT");

        // Assert
        result.Should().Be(BalanceTransactionType.Credit);
    }

    [Fact]
    public void Parse_ShouldReturnDebit_WhenTypeIsDebit()
    {
        // Act
        var result = TransactionTypeParser.Parse("DEBIT");

        // Assert
        result.Should().Be(BalanceTransactionType.Debit);
    }

    [Fact]
    public void Parse_ShouldIgnoreCase()
    {
        // Act
        var result = TransactionTypeParser.Parse("credit");

        // Assert
        result.Should().Be(BalanceTransactionType.Credit);
    }

    [Fact]
    public void Parse_ShouldIgnoreSpaces()
    {
        // Act
        var result = TransactionTypeParser.Parse("  DEBIT  ");

        // Assert
        result.Should().Be(BalanceTransactionType.Debit);
    }

    [Fact]
    public void Parse_ShouldThrowInvalidOperationException_WhenTypeIsUnknown()
    {
        // Act
        var act = () => TransactionTypeParser.Parse("TRANSFER");

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Unsupported transaction type: TRANSFER");
    }
}