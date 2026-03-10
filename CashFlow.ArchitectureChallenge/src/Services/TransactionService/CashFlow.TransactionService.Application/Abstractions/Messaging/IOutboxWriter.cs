using CashFlow.BuildingBlocks.Contracts.Messaging;

namespace CashFlow.TransactionService.Application.Abstractions.Messaging;

public interface IOutboxWriter
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);
}