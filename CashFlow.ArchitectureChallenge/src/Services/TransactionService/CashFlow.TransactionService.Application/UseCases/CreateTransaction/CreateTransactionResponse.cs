namespace CashFlow.TransactionService.Application.UseCases.CreateTransaction;

public sealed class CreateTransactionResponse
{
    public Guid TransactionId { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}