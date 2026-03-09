using CashFlow.TransactionService.Domain.Entities;

namespace CashFlow.TransactionService.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);
}