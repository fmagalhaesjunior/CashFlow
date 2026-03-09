namespace CashFlow.TransactionService.API.Contracts;

public sealed class CreateTransactionRequest
{
    public decimal Amount { get; init; }
    public short Type { get; init; }
    public string Description { get; init; } = string.Empty;
}