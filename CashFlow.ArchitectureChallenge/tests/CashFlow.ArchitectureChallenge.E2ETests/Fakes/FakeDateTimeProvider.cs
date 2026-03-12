using CashFlow.TransactionService.Application.Abstractions;

namespace CashFlow.ArchitectureChallenge.E2ETests.Fakes;

public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; }
}