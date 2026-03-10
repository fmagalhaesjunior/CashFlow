using CashFlow.BuildingBlocks.Domain.Abstractions;

namespace CashFlow.BalanceService.Domain.Entities;

public sealed class DailyBalance : AggregateRoot
{
    private DailyBalance()
    {
    }

    private DailyBalance(DateOnly date)
    {
        Date = date;
        TotalCredit = 0;
        TotalDebit = 0;
        Balance = 0;
    }

    public DateOnly Date { get; private set; }
    public decimal TotalCredit { get; private set; }
    public decimal TotalDebit { get; private set; }
    public decimal Balance { get; private set; }

    public static DailyBalance Create(DateOnly date)
    {
        return new DailyBalance(date);
    }

    public void ApplyCredit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Credit amount must be greater than zero.");
        }

        TotalCredit += amount;
        Recalculate();
    }

    public void ApplyDebit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Debit amount must be greater than zero.");
        }

        TotalDebit += amount;
        Recalculate();
    }

    private void Recalculate()
    {
        Balance = TotalCredit - TotalDebit;
    }
}