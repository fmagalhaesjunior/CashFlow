using CashFlow.BalanceService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.ArchitectureChallenge.E2ETests.Web;

public sealed class CustomBalanceE2EWebApplicationFactory
    : WebApplicationFactory<BalanceService.API.Program>
{
    private readonly string _connectionString;
    private readonly string _rabbitMqHost;
    private readonly int _rabbitMqPort;

    public CustomBalanceE2EWebApplicationFactory(
        string connectionString,
        string rabbitMqHost,
        int rabbitMqPort)
    {
        _connectionString = connectionString;
        _rabbitMqHost = rabbitMqHost;
        _rabbitMqPort = rabbitMqPort;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var config = new Dictionary<string, string?>
            {
                ["ConnectionStrings:BalanceDb"] = _connectionString,
                ["RabbitMq:HostName"] = _rabbitMqHost,
                ["RabbitMq:Port"] = _rabbitMqPort.ToString(),
                ["RabbitMq:UserName"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["RabbitMq:VirtualHost"] = "/",
                ["RabbitMq:ExchangeName"] = "cashflow.events",
                ["RabbitMq:QueueName"] = "balance.transaction.created",
                ["RabbitMq:RoutingKey"] = "transaction.created",
                ["RabbitMq:RetryExchangeName"] = "cashflow.retry",
                ["RabbitMq:RetryQueueName"] = "balance.transaction.created.retry",
                ["RabbitMq:RetryRoutingKey"] = "transaction.created.retry",
                ["RabbitMq:DeadLetterExchangeName"] = "cashflow.dlq",
                ["RabbitMq:DeadLetterQueueName"] = "balance.transaction.created.dlq",
                ["RabbitMq:DeadLetterRoutingKey"] = "transaction.created.dlq",
                ["RabbitMq:PrefetchCount"] = "10",
                ["RabbitMq:RetryDelayMilliseconds"] = "1000",
                ["RabbitMq:MaxConsumerRetries"] = "3"
            };

            configBuilder.AddInMemoryCollection(config);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BalanceDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<BalanceDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BalanceDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }
}