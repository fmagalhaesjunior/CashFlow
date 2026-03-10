using CashFlow.TransactionService.Application.Abstractions;
using CashFlow.TransactionService.Application.Abstractions.Messaging;
using CashFlow.TransactionService.Application.Abstractions.Outbox;
using CashFlow.TransactionService.Application.Abstractions.Persistence;
using CashFlow.TransactionService.Infra.BackgroundJobs.Outbox;
using CashFlow.TransactionService.Infra.Messaging;
using CashFlow.TransactionService.Infra.Messaging.RabbitMq;
using CashFlow.TransactionService.Infra.Persistence;
using CashFlow.TransactionService.Infra.Repositories;
using CashFlow.TransactionService.Infra.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.TransactionService.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddTransactionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TransactionDb");

        services.AddDbContext<TransactionDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        services.Configure<OutboxPublisherOptions>(
            configuration.GetSection(OutboxPublisherOptions.SectionName));

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IOutboxWriter, EfCoreOutboxWriter>();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IEventRoutingKeyResolver, EventRoutingKeyResolver>();
        services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        services.AddHostedService<OutboxPublisherWorker>();

        return services;
    }
}