using CashFlow.TransactionService.Domain.Entities;
using CashFlow.TransactionService.Infra.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.TransactionService.Infra.Persistence;

public sealed class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}