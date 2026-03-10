namespace CashFlow.BalanceService.Application.Extensions;

public static class DateTimeExtensions
{
    public static DateTime UtcDateTimeSafe(this DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}