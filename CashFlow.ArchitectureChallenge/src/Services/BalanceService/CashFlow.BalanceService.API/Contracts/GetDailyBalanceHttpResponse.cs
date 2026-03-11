namespace CashFlow.BalanceService.API.Contracts;

public sealed class GetDailyBalanceHttpResponse
{
    public string Date { get; init; } = string.Empty;
    public decimal TotalCredit { get; init; }
    public decimal TotalDebit { get; init; }
    public decimal Balance { get; init; }
}