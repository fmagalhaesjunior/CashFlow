using CashFlow.BalanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.BalanceService.Infrastructure.Persistence;

public sealed class BalanceDbContext : DbContext
{
    public BalanceDbContext(DbContextOptions<BalanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<DailyBalance> DailyBalances => Set<DailyBalance>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BalanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}