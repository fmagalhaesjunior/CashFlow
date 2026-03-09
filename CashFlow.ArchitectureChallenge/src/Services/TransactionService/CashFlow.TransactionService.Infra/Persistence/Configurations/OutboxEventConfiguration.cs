using CashFlow.TransactionService.Infra.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.TransactionService.Infra.Persistence.Configurations;

public sealed class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("outbox_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.Processed)
            .HasColumnName("processed")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        builder.HasIndex(x => new { x.Processed, x.CreatedAt })
            .HasDatabaseName("ix_outbox_events_processed_created_at");
    }
}