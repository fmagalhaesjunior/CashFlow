using CashFlow.BuildingBlocks.Application.Messaging;

namespace CashFlow.TransactionService.Application.UseCases.CreateTransaction;

public sealed class CreateTransactionCommand : ICommand<CreateTransactionResponse>
{
    public decimal Amount { get; init; }
    public short Type { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? CorrelationId { get; init; }
}