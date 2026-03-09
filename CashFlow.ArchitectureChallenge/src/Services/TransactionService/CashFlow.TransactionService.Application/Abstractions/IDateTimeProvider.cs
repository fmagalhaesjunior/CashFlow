namespace CashFlow.TransactionService.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}