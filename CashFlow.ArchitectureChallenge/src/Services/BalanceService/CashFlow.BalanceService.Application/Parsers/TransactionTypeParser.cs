using CashFlow.BalanceService.Application.Models;

namespace CashFlow.BalanceService.Application.Parsers;

public static class TransactionTypeParser
{
    public static BalanceTransactionType Parse(string type)
    {
        return type.Trim().ToUpperInvariant() switch
        {
            "CREDIT" => BalanceTransactionType.Credit,
            "DEBIT" => BalanceTransactionType.Debit,
            _ => throw new InvalidOperationException($"Unsupported transaction type: {type}")
        };
    }
}