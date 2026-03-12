using CashFlow.ArchitectureChallenge.E2ETests.Fakes;
using CashFlow.TransactionService.Application.Abstractions;
using CashFlow.TransactionService.Infra.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CashFlow.ArchitectureChallenge.E2ETests.Web;

public sealed class CustomTransactionE2EWebApplicationFactory
    : WebApplicationFactory<CashFlow.TransactionService.API.Program>
{
    private readonly string _connectionString;
    private readonly string _rabbitMqHost;
    private readonly int _rabbitMqPort;
    private readonly DateTime _fixedUtcNow;

    public CustomTransactionE2EWebApplicationFactory(
        string connectionString,
        string rabbitMqHost,
        int rabbitMqPort,
        DateTime fixedUtcNow)
    {
        _connectionString = connectionString;
        _rabbitMqHost = rabbitMqHost;
        _rabbitMqPort = rabbitMqPort;
        _fixedUtcNow = fixedUtcNow;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var config = new Dictionary<string, string?>
            {
                ["ConnectionStrings:TransactionDb"] = _connectionString,
                ["RabbitMq:HostName"] = _rabbitMqHost,
                ["RabbitMq:Port"] = _rabbitMqPort.ToString(),
                ["RabbitMq:UserName"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["RabbitMq:VirtualHost"] = "/",
                ["RabbitMq:ExchangeName"] = "cashflow.events",
                ["OutboxPublisher:BatchSize"] = "50",
                ["OutboxPublisher:PollingIntervalSeconds"] = "1",
                ["OutboxPublisher:MaxRetries"] = "5",
                ["OutboxPublisher:BaseRetryDelaySeconds"] = "1"
            };

            configBuilder.AddInMemoryCollection(config);
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TransactionDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<TransactionDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });

            services.RemoveAll<IDateTimeProvider>();
            services.AddSingleton<IDateTimeProvider>(new FakeDateTimeProvider(_fixedUtcNow));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }
}