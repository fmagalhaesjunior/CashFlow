using CashFlow.TransactionService.Application.Abstractions;

namespace CashFlow.TransactionService.Infra.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}