using CashFlow.BalanceService.Application.Abstractions.Persistence;
using CashFlow.BalanceService.Application.Abstractions.Queries;
using CashFlow.BalanceService.Infrastructure.BackgroundJobs;
using CashFlow.BalanceService.Infrastructure.Messaging.RabbitMq;
using CashFlow.BalanceService.Infrastructure.Observability;
using CashFlow.BalanceService.Infrastructure.Persistence;
using CashFlow.BalanceService.Infrastructure.Queries;
using CashFlow.BalanceService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.BalanceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBalanceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BalanceDb");

        services.AddDbContext<BalanceDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.Configure<RabbitMqConsumerOptions>(
            configuration.GetSection(RabbitMqConsumerOptions.SectionName));

        services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        services.AddScoped<IProcessedEventRepository, ProcessedEventRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IDailyBalanceReadRepository, DailyBalanceReadRepository>();

        services.AddSingleton<IUniqueConstraintDetector, PostgresUniqueConstraintDetector>();
        services.AddSingleton<BalanceConsumerMetrics>();
        services.AddSingleton<IConsumerFailurePublisher, ConsumerFailurePublisher>();

        services.AddHostedService<TransactionCreatedConsumerWorker>();

        return services;
    }
}