using CashFlow.BalanceService.Application.Abstractions.Persistence;
using CashFlow.BalanceService.Domain.Entities;
using CashFlow.BalanceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.BalanceService.Infrastructure.Repositories;

public sealed class ProcessedEventRepository : IProcessedEventRepository
{
    private readonly BalanceDbContext _dbContext;

    public ProcessedEventRepository(BalanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return _dbContext.ProcessedEvents
            .AnyAsync(x => x.EventId == eventId, cancellationToken);
    }

    public async Task AddAsync(ProcessedEvent processedEvent, CancellationToken cancellationToken)
    {
        await _dbContext.ProcessedEvents.AddAsync(processedEvent, cancellationToken);
    }
}