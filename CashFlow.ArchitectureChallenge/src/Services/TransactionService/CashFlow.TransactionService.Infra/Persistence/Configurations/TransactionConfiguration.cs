using CashFlow.TransactionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.TransactionService.Infra.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.Timestamp)
            .HasDatabaseName("ix_transactions_timestamp");
    }
}