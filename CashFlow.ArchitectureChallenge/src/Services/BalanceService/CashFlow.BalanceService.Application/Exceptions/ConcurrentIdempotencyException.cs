namespace CashFlow.BalanceService.Application.Exceptions;

public sealed class ConcurrentIdempotencyException : Exception
{
    public ConcurrentIdempotencyException(Guid eventId, Exception? innerException = null)
        : base($"Event '{eventId}' was already processed concurrently.", innerException)
    {
        EventId = eventId;
    }

    public Guid EventId { get; }
}