namespace CashFlow.BalanceService.Application.Abstractions.Persistence;

public interface IUniqueConstraintDetector
{
    bool IsProcessedEventUniqueViolation(Exception exception);
}