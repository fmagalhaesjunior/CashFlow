using CashFlow.BalanceService.API.Contracts;
using CashFlow.BalanceService.Application.Queries.GetDailyBalance;

namespace CashFlow.BalanceService.API.Mappers;

public static class DailyBalanceResponseMapper
{
    public static GetDailyBalanceHttpResponse ToHttpResponse(GetDailyBalanceResponse response)
    {
        return new GetDailyBalanceHttpResponse
        {
            Date = response.Date.ToString("yyyy-MM-dd"),
            TotalCredit = response.TotalCredit,
            TotalDebit = response.TotalDebit,
            Balance = response.Balance
        };
    }
}