namespace CashFlow.BalanceService.Application.Queries.GetDailyBalance;

public sealed class GetDailyBalanceResponse
{
    public DateOnly Date { get; init; }
    public decimal TotalCredit { get; init; }
    public decimal TotalDebit { get; init; }
    public decimal Balance { get; init; }
}