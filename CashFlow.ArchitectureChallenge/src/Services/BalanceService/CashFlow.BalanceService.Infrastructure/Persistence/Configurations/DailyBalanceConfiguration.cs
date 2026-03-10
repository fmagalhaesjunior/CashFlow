using CashFlow.BalanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.BalanceService.Infrastructure.Persistence.Configurations;

public sealed class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
{
    public void Configure(EntityTypeBuilder<DailyBalance> builder)
    {
        builder.ToTable("daily_balance");

        builder.HasKey(x => x.Date);

        builder.Property(x => x.Date)
            .HasColumnName("date")
            .HasConversion(
                date => date.ToDateTime(TimeOnly.MinValue),
                value => DateOnly.FromDateTime(value))
            .HasColumnType("date");

        builder.Property(x => x.TotalCredit)
            .HasColumnName("total_credit")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TotalDebit)
            .HasColumnName("total_debit")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Balance)
            .HasColumnName("balance")
            .HasPrecision(18, 2)
            .IsRequired();
    }
}