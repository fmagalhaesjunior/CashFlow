using CashFlow.TransactionService.Application.Abstractions.Persistence;
using CashFlow.TransactionService.Domain.Entities;
using CashFlow.TransactionService.Infra.Persistence;

namespace CashFlow.TransactionService.Infra.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _dbContext;

    public TransactionRepository(TransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
    }
}