using CashFlow.BalanceService.Application.Abstractions.Persistence;
using PostgresException = Npgsql.PostgresException;

namespace CashFlow.BalanceService.Infrastructure.Persistence;

public sealed class PostgresUniqueConstraintDetector : IUniqueConstraintDetector
{
    private const string UniqueViolationSqlState = "23505";

    public bool IsProcessedEventUniqueViolation(Exception exception)
    {
        var current = exception;

        while (current is not null)
        {
            if (current is PostgresException postgresException &&
                postgresException.SqlState == UniqueViolationSqlState &&
                string.Equals(postgresException.ConstraintName, "pk_processed_events", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = current.InnerException!;
        }

        return false;
    }
}